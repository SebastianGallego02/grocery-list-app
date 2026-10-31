using System.Security.Claims;
using GroceryList.Application.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GroceryList.Api.Auth;

/// <summary>
/// Resolves the current user from the validated access token's "sub" claim.
/// </summary>
internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        IsAuthenticated && Guid.TryParse(Principal!.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId)
            ? userId
            // Endpoints that need the user require authentication, so reaching this is a programming error.
            : throw new InvalidOperationException("There is no authenticated user with a valid 'sub' claim.");

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}
