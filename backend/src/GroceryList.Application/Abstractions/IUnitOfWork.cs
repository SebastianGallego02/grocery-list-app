namespace GroceryList.Application.Abstractions;

public interface IUnitOfWork
{
    /// <exception cref="UniqueConstraintViolationException">A write violates a unique constraint.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
