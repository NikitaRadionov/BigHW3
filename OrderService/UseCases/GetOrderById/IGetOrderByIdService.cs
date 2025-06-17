namespace OrderService.UseCases.GetOrderById;

public interface IGetOrderByIdService
{
    Task<GetOrderByIdResponse?> GetOrderByIdAsync(GetOrderByIdRequest request, CancellationToken cancellationToken);
}