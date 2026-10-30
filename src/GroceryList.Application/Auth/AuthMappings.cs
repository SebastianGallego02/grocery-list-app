using GroceryList.Application.Abstractions;
using GroceryList.Domain.Entities;

namespace GroceryList.Application.Auth;

internal static class AuthMappings
{
    public static AuthResponse ToAuthResponse(this User user, AccessToken token) =>
        new(token.Token, token.ExpiresAtUtc, user.Username, user.FullName);
}
