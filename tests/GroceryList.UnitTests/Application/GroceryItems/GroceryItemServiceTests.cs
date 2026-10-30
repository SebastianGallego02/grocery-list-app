using GroceryList.Application.Abstractions;
using GroceryList.Application.Common;
using GroceryList.Application.GroceryItems;
using GroceryList.Domain.Entities;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace GroceryList.UnitTests.Application.GroceryItems;

public sealed class GroceryItemServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid CurrentUserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private readonly IGroceryItemRepository _repository = Substitute.For<IGroceryItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Dictionary<Guid, List<GroceryItem>> _itemsByOwner = [];
    private readonly GroceryItemService _sut;

    public GroceryItemServiceTests()
    {
        _currentUser.UserId.Returns(CurrentUserId);
        _currentUser.IsAuthenticated.Returns(true);

        _sut = new GroceryItemService(_repository, _unitOfWork, _currentUser, new FakeTimeProvider(Now));
    }

    // ---------- GetMyItems ----------

    [Fact]
    public async Task GetMyItems_ReturnsCurrentUsersItemsMapped()
    {
        var milk = GivenItemOwnedBy(CurrentUserId, "Milk", quantity: 2);
        var bread = GivenItemOwnedBy(CurrentUserId, "Bread");
        bread.PutInBasket();

        var result = await _sut.GetMyItemsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [
                new GroceryItemResponse(milk.Id, "Milk", 2, false, Now),
                new GroceryItemResponse(bread.Id, "Bread", 1, true, Now),
            ],
            result.Value);
    }

    [Fact]
    public async Task GetMyItems_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _sut.GetMyItemsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetMyItems_DoesNotReturnAnotherUsersItems()
    {
        GivenItemOwnedBy(OtherUserId, "Secret item");

        var result = await _sut.GetMyItemsAsync(CancellationToken.None);

        Assert.Empty(result.Value);
        await _repository.Received(1).GetAllForUserAsync(CurrentUserId, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().GetAllForUserAsync(OtherUserId, Arg.Any<CancellationToken>());
    }

    // ---------- Add ----------

    [Fact]
    public async Task Add_WithValidRequest_PersistsItemForCurrentUser()
    {
        var result = await _sut.AddAsync(new AddGroceryItemRequest("  Milk  ", 3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Milk", result.Value.Name);
        Assert.Equal(3, result.Value.Quantity);
        Assert.False(result.Value.IsInBasket);
        Assert.Equal(Now, result.Value.CreatedAtUtc);

        await _repository.Received(1).AddAsync(
            Arg.Is<GroceryItem>(i =>
                i.Id == result.Value.Id &&
                i.UserId == CurrentUserId &&
                i.Name == "Milk" &&
                i.Quantity == 3),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_WithoutQuantity_DefaultsToOne()
    {
        var result = await _sut.AddAsync(new AddGroceryItemRequest("Milk"), CancellationToken.None);

        Assert.Equal(1, result.Value.Quantity);
    }

    [Fact]
    public async Task Add_WithDuplicateName_ReturnsConflictAndDoesNotPersist()
    {
        _repository.ExistsByNameForUserAsync("Milk", CurrentUserId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.AddAsync(new AddGroceryItemRequest(" Milk "), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.GroceryItems.DuplicateName, result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        await AssertNothingAddedAsync();
    }

    [Fact]
    public async Task Add_ChecksDuplicatesOnlyWithinCurrentUsersList()
    {
        // Another user already has "Milk"; that must not block the current user.
        _repository.ExistsByNameForUserAsync("Milk", OtherUserId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.AddAsync(new AddGroceryItemRequest("Milk"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).ExistsByNameForUserAsync("Milk", CurrentUserId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(InvalidAddRequests))]
    public async Task Add_WithInvalidInput_ReturnsValidationErrorWithoutTouchingRepository(
        AddGroceryItemRequest request, Error expected)
    {
        var result = await _sut.AddAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expected, result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        await _repository.DidNotReceiveWithAnyArgs().ExistsByNameForUserAsync(default!, default, default);
        await AssertNothingAddedAsync();
    }

    public static TheoryData<AddGroceryItemRequest, Error> InvalidAddRequests() => new()
    {
        { new AddGroceryItemRequest(null!), Errors.GroceryItems.NameRequired },
        { new AddGroceryItemRequest(""), Errors.GroceryItems.NameRequired },
        { new AddGroceryItemRequest("   "), Errors.GroceryItems.NameRequired },
        { new AddGroceryItemRequest(new string('a', GroceryItem.NameMaxLength + 1)), Errors.GroceryItems.NameTooLong },
        { new AddGroceryItemRequest("Milk", 0), Errors.GroceryItems.QuantityInvalid },
        { new AddGroceryItemRequest("Milk", -5), Errors.GroceryItems.QuantityInvalid },
    };

    // ---------- Remove ----------

    [Fact]
    public async Task Remove_OwnItem_RemovesAndSaves()
    {
        var item = GivenItemOwnedBy(CurrentUserId, "Milk");

        var result = await _sut.RemoveAsync(item.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Remove(item);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remove_UnknownItem_ReturnsNotFound()
    {
        var result = await _sut.RemoveAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(Errors.GroceryItems.NotFound, result.Error);
        AssertNotRemoved();
        await AssertNotSavedAsync();
    }

    [Fact]
    public async Task Remove_AnotherUsersItem_ReturnsNotFoundAndDoesNotRemove()
    {
        var foreignItem = GivenItemOwnedBy(OtherUserId, "Secret item");

        var result = await _sut.RemoveAsync(foreignItem.Id, CancellationToken.None);

        Assert.Equal(Errors.GroceryItems.NotFound, result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        await _repository.Received(1).GetByIdForUserAsync(foreignItem.Id, CurrentUserId, Arg.Any<CancellationToken>());
        AssertNotRemoved();
        await AssertNotSavedAsync();
    }

    // ---------- PutInBasket ----------

    [Fact]
    public async Task PutInBasket_OwnItem_TagsAndSaves()
    {
        var item = GivenItemOwnedBy(CurrentUserId, "Milk");

        var result = await _sut.PutInBasketAsync(item.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsInBasket);
        Assert.True(item.IsInBasket);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PutInBasket_UnknownItem_ReturnsNotFound()
    {
        var result = await _sut.PutInBasketAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(Errors.GroceryItems.NotFound, result.Error);
        await AssertNotSavedAsync();
    }

    [Fact]
    public async Task PutInBasket_AnotherUsersItem_ReturnsNotFoundAndLeavesItUntouched()
    {
        var foreignItem = GivenItemOwnedBy(OtherUserId, "Secret item");

        var result = await _sut.PutInBasketAsync(foreignItem.Id, CancellationToken.None);

        Assert.Equal(Errors.GroceryItems.NotFound, result.Error);
        Assert.False(foreignItem.IsInBasket);
        await AssertNotSavedAsync();
    }

    // ---------- RemoveFromBasket ----------

    [Fact]
    public async Task RemoveFromBasket_OwnItem_UntagsAndSaves()
    {
        var item = GivenItemOwnedBy(CurrentUserId, "Milk");
        item.PutInBasket();

        var result = await _sut.RemoveFromBasketAsync(item.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsInBasket);
        Assert.False(item.IsInBasket);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFromBasket_UnknownItem_ReturnsNotFound()
    {
        var result = await _sut.RemoveFromBasketAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(Errors.GroceryItems.NotFound, result.Error);
        await AssertNotSavedAsync();
    }

    [Fact]
    public async Task RemoveFromBasket_AnotherUsersItem_ReturnsNotFoundAndLeavesItUntouched()
    {
        var foreignItem = GivenItemOwnedBy(OtherUserId, "Secret item");
        foreignItem.PutInBasket();

        var result = await _sut.RemoveFromBasketAsync(foreignItem.Id, CancellationToken.None);

        Assert.Equal(Errors.GroceryItems.NotFound, result.Error);
        Assert.True(foreignItem.IsInBasket);
        await AssertNotSavedAsync();
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Arranges the repository to behave like the real one: lookups only succeed for the owner's id.
    /// </summary>
    private GroceryItem GivenItemOwnedBy(Guid ownerId, string name, int quantity = 1)
    {
        var item = GroceryItem.Create(ownerId, name, quantity, Now);

        if (!_itemsByOwner.TryGetValue(ownerId, out var items))
        {
            items = [];
            _itemsByOwner[ownerId] = items;
            _repository.GetAllForUserAsync(ownerId, Arg.Any<CancellationToken>())
                .Returns(_ => (IReadOnlyList<GroceryItem>)items.ToList());
        }

        items.Add(item);
        _repository.GetByIdForUserAsync(item.Id, ownerId, Arg.Any<CancellationToken>()).Returns(item);

        return item;
    }

    private async Task AssertNothingAddedAsync()
    {
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await AssertNotSavedAsync();
    }

    private void AssertNotRemoved() => _repository.DidNotReceiveWithAnyArgs().Remove(default!);

    private Task AssertNotSavedAsync() => _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
}
