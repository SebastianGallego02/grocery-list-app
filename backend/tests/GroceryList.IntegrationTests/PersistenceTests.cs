using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;
using GroceryList.IntegrationTests.Fixtures;
using GroceryList.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GroceryList.IntegrationTests;

/// <summary>
/// Database-level guarantees that unit tests cannot cover: the unique indexes are case-insensitive
/// under SQL Server's default collation, and violations surface as UniqueConstraintViolationException.
/// These bypass the services' pre-checks, simulating a request that loses a race.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class PersistenceTests(GroceryListApiFactory factory)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public async Task DuplicateUsernameIgnoringCase_ThrowsUniqueConstraintViolation()
    {
        var username = ApiTestHelpers.NewUsername();
        await SaveAsync(db => db.Users.Add(User.Create(username, "First", "hash", Now)));

        var exception = await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            SaveAsync(db => db.Users.Add(User.Create(username.ToUpperInvariant(), "Second", "hash", Now))));

        AssertCausedByUniqueIndex(exception);
    }

    [Fact]
    public async Task DuplicateItemNameIgnoringCaseForSameUser_ThrowsUniqueConstraintViolation()
    {
        var user = User.Create(ApiTestHelpers.NewUsername(), "Owner", "hash", Now);
        await SaveAsync(db =>
        {
            db.Users.Add(user);
            db.GroceryItems.Add(GroceryItem.Create(user.Id, "Milk", 1, Now));
        });

        var exception = await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            SaveAsync(db => db.GroceryItems.Add(GroceryItem.Create(user.Id, "milk", 1, Now))));

        AssertCausedByUniqueIndex(exception);
    }

    [Fact]
    public async Task SameItemNameForDifferentUsers_IsAllowed()
    {
        var first = User.Create(ApiTestHelpers.NewUsername(), "First", "hash", Now);
        var second = User.Create(ApiTestHelpers.NewUsername(), "Second", "hash", Now);

        await SaveAsync(db =>
        {
            db.Users.AddRange(first, second);
            db.GroceryItems.Add(GroceryItem.Create(first.Id, "Milk", 1, Now));
            db.GroceryItems.Add(GroceryItem.Create(second.Id, "Milk", 1, Now));
        });
    }

    [Fact]
    public async Task DeletingAUser_CascadesToTheirItems()
    {
        var user = User.Create(ApiTestHelpers.NewUsername(), "Owner", "hash", Now);
        await SaveAsync(db =>
        {
            db.Users.Add(user);
            db.GroceryItems.Add(GroceryItem.Create(user.Id, "Milk", 1, Now));
        });

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Users.Where(u => u.Id == user.Id).ExecuteDeleteAsync();

        Assert.False(await dbContext.GroceryItems.AnyAsync(item => item.UserId == user.Id));
    }

    // Each save runs in a fresh scope (a new DbContext), like separate requests.
    private async Task SaveAsync(Action<AppDbContext> stage)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        stage(dbContext);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    private static void AssertCausedByUniqueIndex(UniqueConstraintViolationException exception)
    {
        var sqlException = Assert.IsType<SqlException>(exception.InnerException?.InnerException);
        Assert.Contains(sqlException.Number, new[] { 2601, 2627 });
    }
}
