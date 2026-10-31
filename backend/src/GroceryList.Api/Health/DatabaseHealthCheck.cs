using GroceryList.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GroceryList.Api.Health;

/// <summary>
/// Reports whether SQL Server is reachable. Written by hand to avoid an extra health check package.
/// </summary>
internal sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Database is reachable.")
                : HealthCheckResult.Unhealthy("Database is not reachable.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Database check failed.", exception);
        }
    }
}
