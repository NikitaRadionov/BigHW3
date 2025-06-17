namespace OrderService.UseCases.CreateOrder;

public sealed record CreateOrderResponse(Guid OrderId, string Status);