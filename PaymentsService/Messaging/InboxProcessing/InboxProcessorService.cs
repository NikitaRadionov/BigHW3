using PaymentsService.DTOs.Messaging;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;
using System.Text.Json;

namespace PaymentsService.Messaging
{
    public class InboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InboxProcessorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                var unprocessed = await dbContext.InboxMessages
                    .Where(m => !m.Processed)
                    .OrderBy(m => m.ReceivedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var msg in unprocessed)
                {
                    using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                    try
                    {
                        var dto = JsonSerializer.Deserialize<PaymentRequestDto>(msg.Payload)
                                  ?? throw new Exception("Invalid payload");

                        if (!Guid.TryParse(dto.UserId, out var userGuid))
                        {
                            throw new Exception($"Invalid UserId format: {dto.UserId}");
                        }

                        var account = await dbContext.Accounts
                            .FromSqlRaw("SELECT * FROM \"Accounts\" WHERE \"UserId\" = {0} FOR UPDATE", userGuid)
                            .FirstOrDefaultAsync(stoppingToken);

                        var result = new PaymentResultDto
                        {
                            OrderId = dto.OrderId
                        };

                        if (account == null)
                        {
                            result.Status = "failed";
                            result.Reason = "account_not_found";
                        }
                        else if (account.Balance < dto.Amount)
                        {
                            result.Status = "failed";
                            result.Reason = "insufficient_funds";
                        }
                        else
                        {
                            account.Balance -= dto.Amount;
                            result.Status = "success";
                        }

                        dbContext.OutboxMessages.Add(new OutboxMessage
                        {
                            Id = Guid.NewGuid(),
                            OccurredOn = DateTime.UtcNow,
                            Payload = JsonSerializer.Serialize(result),
                            Sent = false
                        });

                        msg.Processed = true;
                        await dbContext.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken);

                        Console.WriteLine($"[PaymentsService] Processed order {dto.OrderId} for user {dto.UserId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PaymentsService] Failed to process inbox message: {ex.Message}");
                        await transaction.RollbackAsync(stoppingToken);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}