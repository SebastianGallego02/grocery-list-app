using GroceryList.Application.Abstractions;
using GroceryList.Application.Common;

namespace GroceryList.Application.Users;

public sealed class UserService(IUserRepository userRepository, ICurrentUser currentUser)
{
    public async Task<Result<UserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUser.UserId, cancellationToken);

        return user is null
            ? Errors.Users.CurrentUserNotFound
            : user.ToUserResponse();
    }
}
