namespace PaymentsService.UseCases.GetBalance;

public interface IGetBalanceService
{
    Task<GetBalanceResponse?> GetBalanceAsync(GetBalanceRequest request, CancellationToken cancellationToken);
}