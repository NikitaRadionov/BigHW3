using Microsoft.EntityFrameworkCore;
using OrdersService.Database;
using OrdersService.Models;
using RabbitMQ.Client;
using System.Text;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _services;
    private IConnection? _connection;
    private IModel? _channel;
    private const int MaxRetryCount = 5;

    public OutboxProcessorService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureRabbitMqConnectedWithRetryAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                var messages = await db.OutboxMessages
                    .Where(m => m.Status == OutboxMessageStatus.New || m.Status == OutboxMessageStatus.Failed)
                    .Where(m => m.RetryCount < MaxRetryCount)
                    .OrderBy(m => m.OccurredOn)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        var body = Encoding.UTF8.GetBytes(message.Payload);
                        var properties = _channel!.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.MessageId = message.Id.ToString();

                        _channel.BasicPublish(
                            exchange: "",
                            routingKey: "orders",
                            basicProperties: properties,
                            body: body);

                        message.Status = OutboxMessageStatus.Processed;
                        message.ProcessedAt = DateTime.UtcNow;

                    }
                    catch (Exception ex)
                    {
                        message.Status = OutboxMessageStatus.Failed;
                        message.RetryCount++;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task EnsureRabbitMqConnectedWithRetryAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        int attempt = 0;

        while (!cancellationToken.IsCancellationRequested && attempt < maxAttempts)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "rabbitmq",
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "orders",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                return;
            }
            catch (Exception ex)
            {
                attempt++;
                await Task.Delay(2000, cancellationToken);
            }
        }

        throw new Exception("OutboxProcessorService: Failed to connect to RabbitMQ after multiple attempts.");
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}