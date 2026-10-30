using GroceryList.Domain.Entities;

namespace GroceryList.UnitTests.Domain;

public sealed class GroceryItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsPropertiesAndIsNotInBasket()
    {
        var item = GroceryItem.Create(UserId, "  Milk  ", 2, Now);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(UserId, item.UserId);
        Assert.Equal("Milk", item.Name);
        Assert.Equal(2, item.Quantity);
        Assert.False(item.IsInBasket);
        Assert.Equal(Now, item.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        Assert.Throws<ArgumentException>(() => GroceryItem.Create(Guid.Empty, "Milk", 1, Now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingName_Throws(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => GroceryItem.Create(UserId, name!, 1, Now));
    }

    [Fact]
    public void Create_WithNameTooLong_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GroceryItem.Create(UserId, new string('a', GroceryItem.NameMaxLength + 1), 1, Now));
    }

    [Fact]
    public void Create_WithNameAtMaxLength_Succeeds()
    {
        var item = GroceryItem.Create(UserId, new string('a', GroceryItem.NameMaxLength), 1, Now);

        Assert.Equal(GroceryItem.NameMaxLength, item.Name.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithQuantityBelowMinimum_Throws(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GroceryItem.Create(UserId, "Milk", quantity, Now));
    }

    [Fact]
    public void PutInBasket_TagsItem()
    {
        var item = GroceryItem.Create(UserId, "Milk", 1, Now);

        item.PutInBasket();

        Assert.True(item.IsInBasket);
    }

    [Fact]
    public void RemoveFromBasket_UntagsItem()
    {
        var item = GroceryItem.Create(UserId, "Milk", 1, Now);
        item.PutInBasket();

        item.RemoveFromBasket();

        Assert.False(item.IsInBasket);
    }

    [Fact]
    public void BasketOperations_AreIdempotent()
    {
        var item = GroceryItem.Create(UserId, "Milk", 1, Now);

        item.PutInBasket();
        item.PutInBasket();
        Assert.True(item.IsInBasket);

        item.RemoveFromBasket();
        item.RemoveFromBasket();
        Assert.False(item.IsInBasket);
    }
}
