namespace GroceryList.Domain.Entities;

public sealed class User
{
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 50;
    public const int FullNameMaxLength = 100;

    // Required by EF Core.
    private User()
    {
    }

    private User(Guid id, string username, string fullName, string passwordHash, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Username = username;
        FullName = fullName;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Version 7 GUID stamped with the creation time. See <see cref="GroceryItem.Id"/> for the rationale.
    /// </summary>
    public Guid Id { get; private set; }

    public string Username { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static User Create(string username, string fullName, string passwordHash, DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        username = username.Trim();
        fullName = fullName.Trim();

        if (username.Length is < UsernameMinLength or > UsernameMaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(username),
                $"Username must be between {UsernameMinLength} and {UsernameMaxLength} characters.");
        }

        if (fullName.Length > FullNameMaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fullName),
                $"Full name must be at most {FullNameMaxLength} characters.");
        }

        return new User(Guid.CreateVersion7(createdAtUtc), username, fullName, passwordHash, createdAtUtc);
    }
}
