namespace GroceryList.Domain.Entities;

public sealed class GroceryItem
{
    public const int NameMaxLength = 100;
    public const int MinQuantity = 1;

    // Required by EF Core.
    private GroceryItem()
    {
    }

    private GroceryItem(Guid id, Guid userId, string name, int quantity, DateTimeOffset createdAtUtc)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Quantity = quantity;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public int Quantity { get; private set; }

    /// <summary>
    /// True when the item has been tagged as picked up (put in the basket).
    /// </summary>
    public bool IsInBasket { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static GroceryItem Create(Guid userId, string name, int quantity, DateTimeOffset createdAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        name = name.Trim();

        if (name.Length > NameMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(name), $"Name must be at most {NameMaxLength} characters.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, MinQuantity);

        return new GroceryItem(Guid.NewGuid(), userId, name, quantity, createdAtUtc);
    }

    public void PutInBasket() => IsInBasket = true;

    public void RemoveFromBasket() => IsInBasket = false;
}
