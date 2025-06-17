namespace PaymentsService.UseCases.Deposit;

public interface IDepositService
{
    Task<DepositResponse?> DepositAsync(DepositRequest request, CancellationToken cancellationToken);
}