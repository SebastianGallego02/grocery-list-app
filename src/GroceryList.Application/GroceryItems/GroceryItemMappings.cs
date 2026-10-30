using GroceryList.Domain.Entities;

namespace GroceryList.Application.GroceryItems;

internal static class GroceryItemMappings
{
    public static GroceryItemResponse ToResponse(this GroceryItem item) =>
        new(item.Id, item.Name, item.Quantity, item.IsInBasket, item.CreatedAtUtc);
}
