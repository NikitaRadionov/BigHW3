using System.Text.Json;
using OrdersService.Models;
using OrdersService.Database;

namespace OrderService.UseCases.CreateOrder;

internal sealed class CreateOrderService : ICreateOrderService
{
    private readonly OrderDbContext _context;

    public CreateOrderService(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Amount = request.Amount,
            Description = request.Description,
            Status = OrderStatusType.NEW
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "OrderCreated",
            OccurredOn = DateTime.UtcNow,
            Payload = JsonSerializer.Serialize(new
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Amount = order.Amount
            }),
            Status = OutboxMessageStatus.New
        };

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CreateOrderResponse(order.Id, order.Status.ToString());
    }
}