using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GroceryList.Infrastructure.Security;

/// <summary>
/// Adapter over ASP.NET Core Identity's hasher (PBKDF2 with HMAC-SHA512, salted, versioned format).
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    // Identity's API takes the user for extensibility; the default implementation ignores it.
    public string Hash(string password) => _hasher.HashPassword(null!, password);

    // SuccessRehashNeeded still means the password is correct; rehashing is not implemented yet.
    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) is not PasswordVerificationResult.Failed;
}
