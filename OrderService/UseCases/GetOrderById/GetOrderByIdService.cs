using OrdersService.Database;
using Microsoft.EntityFrameworkCore;

namespace OrderService.UseCases.GetOrderById;

internal sealed class GetOrderByIdService : IGetOrderByIdService
{
    private readonly OrderDbContext _db;

    public GetOrderByIdService(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<GetOrderByIdResponse?> GetOrderByIdAsync(GetOrderByIdRequest request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

        return order is null
            ? null
            : new GetOrderByIdResponse(order.Id, order.Amount, order.Description, order.Status.ToString());
    }
}