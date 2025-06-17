using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;

namespace PaymentsService.UseCases.GetBalance;

public class GetBalanceService : IGetBalanceService
{
    private readonly PaymentsDbContext _db;

    public GetBalanceService(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<GetBalanceResponse?> GetBalanceAsync(GetBalanceRequest request, CancellationToken cancellationToken)
    {
        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        return account == null
            ? null
            : new GetBalanceResponse(account.UserId, account.Balance);
    }
}