using OrdersService.Database;
using Microsoft.EntityFrameworkCore;

namespace OrderService.UseCases.GetUserOrders;

internal sealed class GetUserOrdersService : IGetUserOrdersService
{
    private readonly OrderDbContext _db;

    public GetUserOrdersService(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<GetUserOrdersResponse>> GetOrdersAsync(GetUserOrdersRequest request, CancellationToken cancellationToken)
    {
        return await _db.Orders
            .Where(o => o.UserId == request.UserId)
            .Select(o => new GetUserOrdersResponse(
                o.Id,
                o.Amount,
                o.Description,
                o.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}