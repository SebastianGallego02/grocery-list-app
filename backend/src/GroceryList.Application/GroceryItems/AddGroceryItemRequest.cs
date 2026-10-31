namespace GroceryList.Application.GroceryItems;

public sealed record AddGroceryItemRequest(string Name, int Quantity = 1);
