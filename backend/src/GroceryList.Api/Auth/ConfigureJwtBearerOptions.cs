using GroceryList.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GroceryList.Api.Auth;

/// <summary>
/// Configures token validation from the same validated <see cref="JwtOptions"/> that Infrastructure
/// uses to issue tokens, so issuer, audience and key are defined once.
/// </summary>
internal sealed class ConfigureJwtBearerOptions(IOptions<JwtOptions> jwtOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        var jwt = jwtOptions.Value;

        // Keep JWT claim names as issued ("sub", "unique_name") instead of mapping them to SOAP-era URIs.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.CreateSigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);
}
