using System.Text;
using GroceryList.Domain.Entities;
using GroceryList.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GroceryList.UnitTests.Infrastructure.Security;

public sealed class JwtTokenServiceTests
{
    // Whole seconds: JWT timestamps have second precision.
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly JwtOptions Settings = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SigningKey = "unit-test-signing-key-that-is-long-enough-1234",
        ExpirationMinutes = 30,
    };

    private readonly FakeTimeProvider _timeProvider = new(Now);
    private readonly JwtTokenService _sut;
    private readonly User _user = User.Create("alice", "Alice Smith", "hash", Now);

    public JwtTokenServiceTests()
    {
        _sut = new JwtTokenService(Options.Create(Settings), _timeProvider);
    }

    [Fact]
    public void CreateToken_ContainsUserClaims()
    {
        var jwt = Read(_sut.CreateToken(_user).Token);

        Assert.Equal(_user.Id.ToString(), jwt.Subject);
        Assert.Equal("alice", jwt.GetPayloadValue<string>(JwtRegisteredClaimNames.UniqueName));
        Assert.Equal("Alice Smith", jwt.GetPayloadValue<string>(JwtRegisteredClaimNames.Name));
        Assert.True(Guid.TryParse(jwt.Id, out _), "jti should be a GUID.");
    }

    [Fact]
    public void CreateToken_SetsIssuerAudienceAndAlgorithm()
    {
        var jwt = Read(_sut.CreateToken(_user).Token);

        Assert.Equal(Settings.Issuer, jwt.Issuer);
        Assert.Equal([Settings.Audience], jwt.Audiences);
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Alg);
    }

    [Fact]
    public void CreateToken_UsesTimeProviderForTimestampsAndExpiration()
    {
        var accessToken = _sut.CreateToken(_user);
        var jwt = Read(accessToken.Token);

        var expectedExpiry = Now.AddMinutes(Settings.ExpirationMinutes);
        Assert.Equal(expectedExpiry, accessToken.ExpiresAtUtc);
        Assert.Equal(expectedExpiry.UtcDateTime, jwt.ValidTo);
        Assert.Equal(Now.UtcDateTime, jwt.IssuedAt);
        Assert.Equal(Now.UtcDateTime, jwt.ValidFrom);
    }

    [Fact]
    public void CreateToken_AfterTimeAdvances_ExpiresLater()
    {
        var first = _sut.CreateToken(_user);

        _timeProvider.Advance(TimeSpan.FromMinutes(10));
        var second = _sut.CreateToken(_user);

        Assert.Equal(first.ExpiresAtUtc.AddMinutes(10), second.ExpiresAtUtc);
    }

    [Fact]
    public void CreateToken_GeneratesUniqueJtiPerToken()
    {
        var first = Read(_sut.CreateToken(_user).Token);
        var second = Read(_sut.CreateToken(_user).Token);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task CreateToken_IsSignedWithConfiguredKey()
    {
        var result = await Validate(_sut.CreateToken(_user).Token, Settings.SigningKey);

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task CreateToken_FailsValidationWithAnotherKey()
    {
        var result = await Validate(_sut.CreateToken(_user).Token, "a-completely-different-signing-key-0987654321");

        Assert.False(result.IsValid);
    }

    private static JsonWebToken Read(string token) => new JsonWebTokenHandler().ReadJsonWebToken(token);

    private static Task<TokenValidationResult> Validate(string token, string signingKey) =>
        new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            // Lifetime is covered with FakeTimeProvider above; the handler validates against the real clock.
            ValidateLifetime = false,
        });
}
