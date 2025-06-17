using Microsoft.EntityFrameworkCore;
using OrdersService.Database;
using OrdersService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using OrderService.DTOs.Messaging;

public class PaymentStatusConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentStatusConsumerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureRabbitMqConnectedWithRetryAsync(stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            try
            {
                var paymentResult = JsonSerializer.Deserialize<PaymentResultDto>(json);
                if (paymentResult == null)
                {
                    Console.WriteLine("[OrderService] Received null paymentResult.");
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                if (!Guid.TryParse(paymentResult.OrderId, out var orderId))
                {
                    Console.WriteLine($"[OrderService] Invalid OrderId: {paymentResult.OrderId}");
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, stoppingToken);
                if (order == null)
                {
                    Console.WriteLine($"[OrderService] Order not found: {orderId}");
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                order.Status = paymentResult.Status == "success"
                    ? OrderStatusType.FINISHED
                    : OrderStatusType.CANCELLED;

                await db.SaveChangesAsync(stoppingToken);

                Console.WriteLine($"[OrderService] Order {order.Id} updated to status {order.Status}");

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] Error processing payment status: {ex.Message}");
            }
        };

        _channel!.BasicConsume(
            queue: "payment_status",
            autoAck: false,
            consumer: consumer
        );

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
                var factory = new ConnectionFactory { HostName = "rabbitmq", DispatchConsumersAsync = true };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "payment_status",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                Console.WriteLine("[OrderService] Connected to RabbitMQ");
                return;
            }
            catch (Exception ex)
            {
                attempt++;
                Console.WriteLine($"[OrderService] RabbitMQ connection failed (attempt {attempt}): {ex.Message}");
                await Task.Delay(2000, cancellationToken);
            }
        }

        throw new Exception("PaymentStatusConsumerService: Failed to connect to RabbitMQ after multiple attempts.");
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}