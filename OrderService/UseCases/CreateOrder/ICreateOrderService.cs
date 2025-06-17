namespace OrderService.UseCases.CreateOrder;

public interface ICreateOrderService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}