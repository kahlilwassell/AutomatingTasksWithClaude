using Microsoft.Extensions.DependencyInjection;

namespace ShipmentTracker.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the schema if it does not exist. This is a deliberate starting point
    /// for a sandbox repo: there are no EF migrations yet. Moving to migrations
    /// (and replacing this with <c>Database.MigrateAsync</c>) is Exercise 1 in the playbook.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShipmentTrackerDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
