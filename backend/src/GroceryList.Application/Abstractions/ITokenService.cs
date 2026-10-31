using GroceryList.Domain.Entities;

namespace GroceryList.Application.Abstractions;

public interface ITokenService
{
    AccessToken CreateToken(User user);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAtUtc);
