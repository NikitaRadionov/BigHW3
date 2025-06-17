namespace OrderService.UseCases.GetOrderById;

public sealed record GetOrderByIdRequest(Guid OrderId, Guid UserId);