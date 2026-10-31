using GroceryList.Domain.Entities;

namespace GroceryList.Application.Abstractions;

/// <summary>
/// Every query is scoped to a user, so one user can never read or change another user's items.
/// </summary>
public interface IGroceryItemRepository
{
    Task<IReadOnlyList<GroceryItem>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a tracked item, or null when it does not exist or belongs to another user.
    /// </summary>
    Task<GroceryItem?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>Name comparison is case-insensitive.</summary>
    Task<bool> ExistsByNameForUserAsync(string name, Guid userId, CancellationToken cancellationToken);

    Task AddAsync(GroceryItem item, CancellationToken cancellationToken);

    void Remove(GroceryItem item);
}
