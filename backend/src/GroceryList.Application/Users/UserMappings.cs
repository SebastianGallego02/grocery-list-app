using GroceryList.Domain.Entities;

namespace GroceryList.Application.Users;

internal static class UserMappings
{
    public static UserResponse ToUserResponse(this User user) => new(user.Id, user.Username, user.FullName);
}
