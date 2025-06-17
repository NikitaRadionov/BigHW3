namespace PaymentsService.UseCases.CreateAccount;
public interface ICreateAccountService
{
    Task<CreateAccountResponse?> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
}