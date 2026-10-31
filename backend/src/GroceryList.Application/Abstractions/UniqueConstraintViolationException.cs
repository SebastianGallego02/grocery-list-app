namespace GroceryList.Application.Abstractions;

/// <summary>
/// Thrown by <see cref="IUnitOfWork.SaveChangesAsync"/> when a write violates a unique constraint.
/// Services check uniqueness first, so this only happens when a concurrent request wins the race;
/// they translate it into the matching Conflict error.
/// </summary>
public sealed class UniqueConstraintViolationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
