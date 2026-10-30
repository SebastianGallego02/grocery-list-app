namespace GroceryList.Application.Common;

/// <summary>
/// Catalogue of the expected business errors, grouped by feature.
/// </summary>
public static class Errors
{
    public static class Auth
    {
        public static readonly Error UsernameTaken =
            Error.Conflict("Auth.UsernameTaken", "The username is already taken.");

        // Deliberately the same error for unknown user and wrong password, so callers cannot probe usernames.
        public static readonly Error InvalidCredentials =
            Error.Unauthorized("Auth.InvalidCredentials", "The username or password is incorrect.");
    }

    public static class GroceryItems
    {
        // Also returned when the item belongs to another user, so its existence is not leaked.
        public static readonly Error NotFound =
            Error.NotFound("GroceryItems.NotFound", "The grocery item was not found.");

        public static readonly Error DuplicateName =
            Error.Conflict("GroceryItems.DuplicateName", "A grocery item with this name already exists in your list.");
    }
}
