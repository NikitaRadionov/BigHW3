using Microsoft.EntityFrameworkCore;

namespace OrdersService.Database;

internal sealed class MigrationRunner : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MigrationRunner(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        db.Database.Migrate();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}