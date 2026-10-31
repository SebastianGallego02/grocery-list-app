using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GroceryList.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Also the unit of work: repositories only stage changes and services commit them
/// through <see cref="IUnitOfWork.SaveChangesAsync"/>, which <see cref="DbContext"/> already implements.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    // SQL Server: 2601 = duplicate key in a unique index, 2627 = unique/primary key constraint violation.
    private const int DuplicateKeyInUniqueIndex = 2601;
    private const int UniqueConstraintViolation = 2627;

    public DbSet<User> Users => Set<User>();

    public DbSet<GroceryItem> GroceryItems => Set<GroceryItem>();

    // Overrides the overload that SaveChangesAsync(CancellationToken) delegates to, so both are covered.
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: DuplicateKeyInUniqueIndex or UniqueConstraintViolation })
        {
            throw new UniqueConstraintViolationException("A unique constraint was violated.", exception);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
