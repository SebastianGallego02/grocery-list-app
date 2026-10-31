namespace GroceryList.Application.Auth;

public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAtUtc, string Username, string FullName);
