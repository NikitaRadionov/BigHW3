namespace OrderService.UseCases.GetUserOrders;

public sealed record GetUserOrdersResponse(
    Guid Id,
    decimal Amount,
    string Description,
    string Status
);