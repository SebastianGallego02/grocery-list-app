using GroceryList.Application.Auth;
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

    [Fact]
    public void AuthServiceDummyHash_IsAcceptedByHasherAndNeverMatches()
    {
        Assert.False(_sut.Verify(Password, AuthService.DummyPasswordHash));
    }

    [Fact]
    public void AuthServiceDummyHash_UsesSameAlgorithmAndIterationsAsFreshHashes()
    {
        // Identity v3 header: format marker (1 byte), PRF (4), iteration count (4), salt length (4).
        // If it drifts from fresh hashes, verifying the dummy no longer costs the same as a real one.
        const int headerLength = 13;
        var dummyHeader = Convert.FromBase64String(AuthService.DummyPasswordHash)[..headerLength];
        var freshHeader = Convert.FromBase64String(_sut.Hash(Password))[..headerLength];

        Assert.Equal(freshHeader, dummyHeader);
    }
}
