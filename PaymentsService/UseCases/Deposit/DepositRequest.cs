namespace PaymentsService.UseCases.Deposit;
public record DepositRequest(Guid UserId, decimal Amount);