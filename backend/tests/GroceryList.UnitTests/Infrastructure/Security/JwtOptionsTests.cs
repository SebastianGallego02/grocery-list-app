using System.ComponentModel.DataAnnotations;
using GroceryList.Infrastructure.Security;

namespace GroceryList.UnitTests.Infrastructure.Security;

public sealed class JwtOptionsTests
{
    [Fact]
    public void ValidOptions_PassValidation()
    {
        Assert.True(IsValid(CreateValid()));
    }

    [Fact]
    public void SigningKey_ShorterThanMinimum_FailsValidation()
    {
        var options = CreateValid();
        options.SigningKey = new string('k', JwtOptions.SigningKeyMinLength - 1);

        Assert.False(IsValid(options));
    }

    [Fact]
    public void SigningKey_AtMinimumLength_PassesValidation()
    {
        var options = CreateValid();
        options.SigningKey = new string('k', JwtOptions.SigningKeyMinLength);

        Assert.True(IsValid(options));
    }

    [Fact]
    public void MissingIssuer_FailsValidation()
    {
        var options = CreateValid();
        options.Issuer = "";

        Assert.False(IsValid(options));
    }

    [Fact]
    public void MissingAudience_FailsValidation()
    {
        var options = CreateValid();
        options.Audience = "";

        Assert.False(IsValid(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(24 * 60 + 1)]
    public void ExpirationMinutes_OutOfRange_FailsValidation(int minutes)
    {
        var options = CreateValid();
        options.ExpirationMinutes = minutes;

        Assert.False(IsValid(options));
    }

    private static JwtOptions CreateValid() => new()
    {
        Issuer = "issuer",
        Audience = "audience",
        SigningKey = new string('k', 64),
        ExpirationMinutes = 60,
    };

    private static bool IsValid(JwtOptions options) =>
        Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true);
}
