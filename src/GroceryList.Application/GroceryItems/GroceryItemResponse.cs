namespace GroceryList.Application.GroceryItems;

public sealed record GroceryItemResponse(
    Guid Id,
    string Name,
    int Quantity,
    bool IsInBasket,
    DateTimeOffset CreatedAtUtc);
