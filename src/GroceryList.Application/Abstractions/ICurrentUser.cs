namespace GroceryList.Application.Abstractions;

/// <summary>
/// The authenticated caller, resolved from the access token. The only trusted source of the user id.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The authenticated user's id. Implementations throw when there is no authenticated user,
    /// because endpoints that need it must require authentication.
    /// </summary>
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}
