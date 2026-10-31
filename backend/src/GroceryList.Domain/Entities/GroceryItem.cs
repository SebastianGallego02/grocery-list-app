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

    /// <summary>
    /// Version 7 GUID stamped with the creation time (from TimeProvider, via <c>createdAtUtc</c>).
    /// </summary>
    /// <remarks>
    /// Random (v4) keys land at random positions in a B-tree index, causing page splits and
    /// fragmentation; v7 keys are time-ordered, so in stores that compare GUIDs in byte order new keys
    /// append at the end. Caveat: SQL Server orders <c>uniqueidentifier</c> starting from the last
    /// 6 bytes, which are random in v7, so on a SQL Server clustered primary key this alone does not
    /// remove fragmentation. Ids are still generated in the Domain so entities are valid before saving.
    /// </remarks>
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

        return new GroceryItem(Guid.CreateVersion7(createdAtUtc), userId, name, quantity, createdAtUtc);
    }

    public void PutInBasket() => IsInBasket = true;

    public void RemoveFromBasket() => IsInBasket = false;
}
