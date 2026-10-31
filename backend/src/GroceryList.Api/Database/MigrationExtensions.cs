using GroceryList.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GroceryList.Api.Database;

internal static class MigrationExtensions
{
    public const string ApplyMigrationsOnStartupKey = "Database:ApplyMigrationsOnStartup";

    /// <summary>
    /// Applies pending EF Core migrations when "Database:ApplyMigrationsOnStartup" is true.
    /// </summary>
    /// <remarks>
    /// Acceptable for this demo (one instance, a throwaway database, zero-setup <c>docker compose up</c>).
    /// Not for production: several instances starting together would race to migrate, the app would need
    /// DDL permissions at runtime, and a failing migration would take the app down instead of failing a
    /// controlled deployment step. There, run a migration bundle or idempotent script from the pipeline.
    /// </remarks>
    public static async Task ApplyMigrationsIfEnabledAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>(ApplyMigrationsOnStartupKey))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        app.Logger.LogInformation("Applying database migrations on startup.");
        await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    }
}
