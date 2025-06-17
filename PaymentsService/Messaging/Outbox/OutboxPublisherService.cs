using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using RabbitMQ.Client;
using System.Text;

namespace PaymentsService.Messaging
{
    public class OutboxPublisherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;

        public OutboxPublisherService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await EnsureRabbitMqConnectedWithRetryAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                var pending = await dbContext.OutboxMessages
                    .Where(m => !m.Sent)
                    .OrderBy(m => m.OccurredOn)
                    .ToListAsync(stoppingToken);

                foreach (var msg in pending)
                {
                    var body = Encoding.UTF8.GetBytes(msg.Payload);
                    var props = _channel!.CreateBasicProperties();
                    props.MessageId = msg.Id.ToString();

                    _channel.BasicPublish(
                        exchange: "",
                        routingKey: "payment_status",
                        basicProperties: props,
                        body: body
                    );

                    msg.Sent = true;
                }

                await dbContext.SaveChangesAsync(stoppingToken);
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
                        queue: "payment_status",
                        durable: true,
                        exclusive: false,
                        autoDelete: false
                    );

                    Console.WriteLine("[OutboxPublisherService] Successfully connected to RabbitMQ.");
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"[OutboxPublisherService] RabbitMQ connection failed (attempt {attempt}): {ex.Message}");
                    await Task.Delay(2000, cancellationToken);
                }
            }

            throw new Exception("OutboxPublisherService: Failed to connect to RabbitMQ after multiple attempts.");
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}