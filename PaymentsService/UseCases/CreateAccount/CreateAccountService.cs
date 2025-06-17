using PaymentsService.Database;
using PaymentsService.Models;

namespace PaymentsService.UseCases.CreateAccount;
public class CreateAccountService : ICreateAccountService
{
    private readonly PaymentsDbContext _db;

    public CreateAccountService(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<CreateAccountResponse?> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var exists = _db.Accounts.Any(a => a.UserId == request.UserId);
        if (exists) return null;

        _db.Accounts.Add(new Account
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Balance = 0
        });

        await _db.SaveChangesAsync(cancellationToken);
        return new CreateAccountResponse("Account created.");
    }
}