using GroceryList.Application.Abstractions;
using GroceryList.Application.Common;
using GroceryList.Domain.Entities;

namespace GroceryList.Application.GroceryItems;

/// <summary>
/// Grocery list use cases. The owner is always the current user from the token; no method accepts a user id,
/// and an item owned by someone else is reported as not found so its existence is not leaked.
/// </summary>
public sealed class GroceryItemService(
    IGroceryItemRepository groceryItemRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<IReadOnlyList<GroceryItemResponse>>> GetMyItemsAsync(CancellationToken cancellationToken)
    {
        var items = await groceryItemRepository.GetAllForUserAsync(currentUser.UserId, cancellationToken);

        return Result.Success<IReadOnlyList<GroceryItemResponse>>(items.Select(item => item.ToResponse()).ToList());
    }

    public async Task<Result<GroceryItemResponse>> AddAsync(
        AddGroceryItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (Validate(request) is { } validationError)
        {
            return validationError;
        }

        var userId = currentUser.UserId;
        var name = request.Name.Trim();

        if (await groceryItemRepository.ExistsByNameForUserAsync(name, userId, cancellationToken))
        {
            return Errors.GroceryItems.DuplicateName;
        }

        var item = GroceryItem.Create(userId, name, request.Quantity, timeProvider.GetUtcNow());

        await groceryItemRepository.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return item.ToResponse();
    }

    public async Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await groceryItemRepository.GetByIdForUserAsync(id, currentUser.UserId, cancellationToken);
        if (item is null)
        {
            return Errors.GroceryItems.NotFound;
        }

        groceryItemRepository.Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public Task<Result<GroceryItemResponse>> PutInBasketAsync(Guid id, CancellationToken cancellationToken) =>
        UpdateAsync(id, item => item.PutInBasket(), cancellationToken);

    public Task<Result<GroceryItemResponse>> RemoveFromBasketAsync(Guid id, CancellationToken cancellationToken) =>
        UpdateAsync(id, item => item.RemoveFromBasket(), cancellationToken);

    private async Task<Result<GroceryItemResponse>> UpdateAsync(
        Guid id,
        Action<GroceryItem> change,
        CancellationToken cancellationToken)
    {
        var item = await groceryItemRepository.GetByIdForUserAsync(id, currentUser.UserId, cancellationToken);
        if (item is null)
        {
            return Errors.GroceryItems.NotFound;
        }

        change(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return item.ToResponse();
    }

    private static Error? Validate(AddGroceryItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Errors.GroceryItems.NameRequired;
        }

        if (request.Name.Trim().Length > GroceryItem.NameMaxLength)
        {
            return Errors.GroceryItems.NameTooLong;
        }

        if (request.Quantity < GroceryItem.MinQuantity)
        {
            return Errors.GroceryItems.QuantityInvalid;
        }

        return null;
    }
}
