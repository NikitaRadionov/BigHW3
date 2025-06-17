namespace OrderService.UseCases.CreateOrder;

public sealed record CreateOrderRequest(
    Guid UserId,
    decimal Amount,
    string Description
);