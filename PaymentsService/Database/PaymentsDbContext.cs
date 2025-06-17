using Microsoft.EntityFrameworkCore;
using PaymentsService.Models;

namespace PaymentsService.Database;

public class PaymentsDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Balance).IsRequired();
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceivedAt).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.Processed).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OccurredOn).IsRequired();
            entity.Property(e => e.Sent).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
        });
    }
}