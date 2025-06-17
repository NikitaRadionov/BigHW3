using PaymentsService.Database;

namespace PaymentsService.UseCases.Deposit;

public class DepositService : IDepositService
{
    private readonly PaymentsDbContext _db;

    public DepositService(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<DepositResponse?> DepositAsync(DepositRequest request, CancellationToken cancellationToken)
    {
        var account = _db.Accounts.FirstOrDefault(a => a.UserId == request.UserId);
        if (account == null) return null;

        account.Balance += request.Amount;
        await _db.SaveChangesAsync(cancellationToken);

        return new DepositResponse(account.UserId, account.Balance);
    }
}