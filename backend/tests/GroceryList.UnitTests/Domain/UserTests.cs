using GroceryList.Domain.Entities;

namespace GroceryList.UnitTests.Domain;

public sealed class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var user = User.Create("  alice  ", "  Alice Smith  ", "hash", Now);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("alice", user.Username);
        Assert.Equal("Alice Smith", user.FullName);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(Now, user.CreatedAtUtc);
    }

    [Fact]
    public void Create_GeneratesVersion7IdStampedWithCreationTime()
    {
        var user = User.Create("alice", "Alice", "hash", Now);

        Assert.Equal(7, user.Id.Version);
        Assert.Equal(Now.ToUnixTimeMilliseconds(), UnixMillisecondsOf(user.Id));
    }

    // The first 48 bits of a v7 GUID (in its big-endian string form) are the Unix timestamp in ms.
    internal static long UnixMillisecondsOf(Guid id) => Convert.ToInt64(id.ToString("N")[..12], 16);

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var first = User.Create("alice", "Alice", "hash", Now);
        var second = User.Create("bobby", "Bob", "hash", Now);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingUsername_Throws(string? username)
    {
        Assert.ThrowsAny<ArgumentException>(() => User.Create(username!, "Alice", "hash", Now));
    }

    [Theory]
    [InlineData(User.UsernameMinLength - 1)]
    [InlineData(User.UsernameMaxLength + 1)]
    public void Create_WithUsernameOutOfRange_Throws(int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => User.Create(new string('a', length), "Alice", "hash", Now));
    }

    [Theory]
    [InlineData(User.UsernameMinLength)]
    [InlineData(User.UsernameMaxLength)]
    public void Create_WithUsernameAtBoundaries_Succeeds(int length)
    {
        var user = User.Create(new string('a', length), "Alice", "hash", Now);

        Assert.Equal(length, user.Username.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingFullName_Throws(string? fullName)
    {
        Assert.ThrowsAny<ArgumentException>(() => User.Create("alice", fullName!, "hash", Now));
    }

    [Fact]
    public void Create_WithFullNameTooLong_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => User.Create("alice", new string('a', User.FullNameMaxLength + 1), "hash", Now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingPasswordHash_Throws(string? passwordHash)
    {
        Assert.ThrowsAny<ArgumentException>(() => User.Create("alice", "Alice", passwordHash!, Now));
    }
}
