using GroceryList.Api.Errors;
using GroceryList.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace GroceryList.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController(UserService userService) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await userService.GetCurrentUserAsync(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : this.Problem(result.Error);
    }
}
