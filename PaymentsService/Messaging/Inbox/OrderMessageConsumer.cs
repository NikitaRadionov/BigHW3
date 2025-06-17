using Microsoft.Extensions.Hosting;
using PaymentsService.Database;
using PaymentsService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PaymentsService.Messaging
{
    public class OrderMessageConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;

        public OrderMessageConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await EnsureRabbitMqConnectedWithRetryAsync(stoppingToken);

            var consumer = new EventingBasicConsumer(_channel!);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var inboxMessage = new InboxMessage
                    {
                        Id = Guid.NewGuid(),
                        MessageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString(),
                        ReceivedAt = DateTime.UtcNow,
                        Payload = message,
                        Processed = false
                    };

                    var exists = await dbContext.InboxMessages
                        .AnyAsync(m => m.MessageId == inboxMessage.MessageId, stoppingToken);

                    if (!exists)
                    {
                        dbContext.InboxMessages.Add(inboxMessage);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        Console.WriteLine($"[OrderConsumer] Stored new message: {inboxMessage.MessageId}");
                        _channel!.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OrderConsumer] Error handling message: {ex.Message}");
                }
            };

            _channel!.BasicConsume(queue: "orders", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
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
                    var factory = new ConnectionFactory { HostName = "rabbitmq" };
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.QueueDeclare(
                        queue: "orders",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    Console.WriteLine("[OrderConsumer] Connected to RabbitMQ");
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"[OrderConsumer] RabbitMQ connection failed (attempt {attempt}): {ex.Message}");
                    await Task.Delay(2000, cancellationToken);
                }
            }

            throw new Exception("OrderMessageConsumer: Failed to connect to RabbitMQ after multiple attempts.");
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}