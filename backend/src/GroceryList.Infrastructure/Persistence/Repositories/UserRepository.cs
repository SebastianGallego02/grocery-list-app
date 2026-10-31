using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroceryList.Infrastructure.Persistence.Repositories;

/// <remarks>
/// Username comparisons rely on SQL Server's default case-insensitive collation.
/// </remarks>
internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken) =>
        context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Username == username, cancellationToken);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.Username == username, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await context.Users.AddAsync(user, cancellationToken);
}
