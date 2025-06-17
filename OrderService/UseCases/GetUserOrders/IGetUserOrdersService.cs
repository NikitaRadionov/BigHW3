namespace OrderService.UseCases.GetUserOrders;

public interface IGetUserOrdersService
{
    Task<IEnumerable<GetUserOrdersResponse>> GetOrdersAsync(GetUserOrdersRequest request, CancellationToken cancellationToken);
}