namespace GroceryList.Application.Auth;

public sealed record RegisterRequest(string Username, string FullName, string Password);
