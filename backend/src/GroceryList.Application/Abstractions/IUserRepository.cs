using GroceryList.Domain.Entities;

namespace GroceryList.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Username comparison is case-insensitive.</summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>Username comparison is case-insensitive.</summary>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}
