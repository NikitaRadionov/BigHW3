using Microsoft.EntityFrameworkCore;
using OrdersService.Models;

namespace OrdersService.Database
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .HasColumnType("varchar(20)");
            });
        }
    }
}