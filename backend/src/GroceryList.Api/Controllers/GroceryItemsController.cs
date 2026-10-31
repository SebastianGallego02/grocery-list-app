using GroceryList.Api.Errors;
using GroceryList.Application.GroceryItems;
using Microsoft.AspNetCore.Mvc;

namespace GroceryList.Api.Controllers;

/// <summary>
/// The current user's grocery list. The owner always comes from the token, never from the request.
/// </summary>
[ApiController]
[Route("api/grocery-items")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GroceryItemsController(GroceryItemService groceryItemService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GroceryItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await groceryItemService.GetMyItemsAsync(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : this.Problem(result.Error);
    }

    [HttpPost]
    [ProducesResponseType<GroceryItemResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(AddGroceryItemRequest request, CancellationToken cancellationToken)
    {
        var result = await groceryItemService.AddAsync(request, cancellationToken);

        return result.IsSuccess
            ? Created($"/api/grocery-items/{result.Value.Id}", result.Value)
            : this.Problem(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        var result = await groceryItemService.RemoveAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.Problem(result.Error);
    }

    [HttpPut("{id:guid}/basket")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutInBasket(Guid id, CancellationToken cancellationToken)
    {
        var result = await groceryItemService.PutInBasketAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.Problem(result.Error);
    }

    [HttpDelete("{id:guid}/basket")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromBasket(Guid id, CancellationToken cancellationToken)
    {
        var result = await groceryItemService.RemoveFromBasketAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.Problem(result.Error);
    }
}
