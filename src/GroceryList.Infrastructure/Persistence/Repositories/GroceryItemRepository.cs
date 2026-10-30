using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroceryList.Infrastructure.Persistence.Repositories;

/// <remarks>
/// Every query filters by <c>userId</c>. Name comparisons rely on SQL Server's default
/// case-insensitive collation.
/// </remarks>
internal sealed class GroceryItemRepository(AppDbContext context) : IGroceryItemRepository
{
    public async Task<IReadOnlyList<GroceryItem>> GetAllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await context.GroceryItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAtUtc)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

    // Tracked on purpose: callers modify or remove the returned item and then save.
    public Task<GroceryItem?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        context.GroceryItems
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

    public Task<bool> ExistsByNameForUserAsync(string name, Guid userId, CancellationToken cancellationToken) =>
        context.GroceryItems.AnyAsync(item => item.UserId == userId && item.Name == name, cancellationToken);

    public async Task AddAsync(GroceryItem item, CancellationToken cancellationToken) =>
        await context.GroceryItems.AddAsync(item, cancellationToken);

    public void Remove(GroceryItem item) => context.GroceryItems.Remove(item);
}
