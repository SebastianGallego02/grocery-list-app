using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GroceryList.Infrastructure.Security;

internal sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;

    // Times come from TimeProvider, so the handler must not stamp its own defaults.
    private readonly JsonWebTokenHandler _handler = new() { SetDefaultTimesOnTokenCreation = false };

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _signingCredentials = new SigningCredentials(_options.CreateSigningKey(), SecurityAlgorithms.HmacSha256);
    }

    public AccessToken CreateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _signingCredentials,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.UniqueName] = user.Username,
                [JwtRegisteredClaimNames.Name] = user.FullName,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            },
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
