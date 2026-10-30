using GroceryList.Application.Auth;
using GroceryList.Domain.Entities;

namespace GroceryList.Application.Common;

/// <summary>
/// Catalogue of the expected business errors, grouped by feature.
/// </summary>
public static class Errors
{
    public static class Auth
    {
        public static readonly Error UsernameRequired =
            Error.Validation("Auth.UsernameRequired", "Username is required.");

        public static readonly Error UsernameLength = Error.Validation(
            "Auth.UsernameLength",
            $"Username must be between {User.UsernameMinLength} and {User.UsernameMaxLength} characters.");

        public static readonly Error FullNameRequired =
            Error.Validation("Auth.FullNameRequired", "Full name is required.");

        public static readonly Error FullNameTooLong = Error.Validation(
            "Auth.FullNameTooLong",
            $"Full name must be at most {User.FullNameMaxLength} characters.");

        public static readonly Error PasswordRequired =
            Error.Validation("Auth.PasswordRequired", "Password is required.");

        public static readonly Error PasswordLength = Error.Validation(
            "Auth.PasswordLength",
            $"Password must be between {AuthService.PasswordMinLength} and {AuthService.PasswordMaxLength} characters.");

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
