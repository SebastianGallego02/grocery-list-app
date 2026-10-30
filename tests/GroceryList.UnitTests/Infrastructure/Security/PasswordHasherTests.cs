using GroceryList.Infrastructure.Security;

namespace GroceryList.UnitTests.Infrastructure.Security;

public sealed class PasswordHasherTests
{
    private const string Password = "Correct-Horse-Battery-Staple";

    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_DoesNotReturnThePlainPassword()
    {
        var hash = _sut.Hash(Password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.DoesNotContain(Password, hash);
    }

    [Fact]
    public void Verify_WithCorrectPassword_Succeeds()
    {
        var hash = _sut.Hash(Password);

        Assert.True(_sut.Verify(Password, hash));
    }

    [Theory]
    [InlineData("wrong-password")]
    [InlineData("correct-horse-battery-staple")]
    [InlineData("")]
    public void Verify_WithWrongPassword_Fails(string attempt)
    {
        var hash = _sut.Hash(Password);

        Assert.False(_sut.Verify(attempt, hash));
    }

    [Fact]
    public void Hash_IsSalted_SoSamePasswordGivesDifferentHashes()
    {
        var first = _sut.Hash(Password);
        var second = _sut.Hash(Password);

        Assert.NotEqual(first, second);
        Assert.True(_sut.Verify(Password, first));
        Assert.True(_sut.Verify(Password, second));
    }
}
