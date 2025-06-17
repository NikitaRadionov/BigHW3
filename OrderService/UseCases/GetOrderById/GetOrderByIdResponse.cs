namespace OrderService.UseCases.GetOrderById;

public sealed record GetOrderByIdResponse(
    Guid Id,
    decimal Amount,
    string Description,
    string Status
);