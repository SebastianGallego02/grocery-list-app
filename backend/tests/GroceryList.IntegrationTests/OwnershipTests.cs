using System.Net;
using GroceryList.IntegrationTests.Fixtures;

namespace GroceryList.IntegrationTests;

/// <summary>
/// User story 3: a user only ever sees and changes their own list. Another user's item behaves
/// exactly like an item that does not exist (404, same error code), so its existence is not leaked.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class OwnershipTests(GroceryListApiFactory factory)
{
    [Fact]
    public async Task UserA_DoesNotSeeUserBsItemsInTheirList()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        await bob.Client.AddItemAsync("Bob's secret item");
        var aliceItem = await alice.Client.AddItemAsync("Alice's item");

        var aliceList = await alice.Client.GetItemsAsync();

        Assert.Equal([aliceItem], aliceList);
    }

    [Fact]
    public async Task UserA_CannotReadUserBsItem()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        var bobItem = await bob.Client.AddItemAsync("Eggs");

        var response = await alice.Client.GetAsync($"/api/grocery-items/{bobItem.Id}");

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
    }

    [Fact]
    public async Task UserA_CannotRemoveUserBsItem()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        var bobItem = await bob.Client.AddItemAsync("Eggs");

        var response = await alice.Client.DeleteAsync($"/api/grocery-items/{bobItem.Id}");

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        Assert.Equal([bobItem], await bob.Client.GetItemsAsync());
    }

    [Fact]
    public async Task UserA_CannotTagUserBsItem()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        var bobItem = await bob.Client.AddItemAsync("Eggs");

        var response = await alice.Client.PutAsync($"/api/grocery-items/{bobItem.Id}/basket", content: null);

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        Assert.False(Assert.Single(await bob.Client.GetItemsAsync()).IsInBasket);
    }

    [Fact]
    public async Task UserA_CannotUntagUserBsItem()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        var bobItem = await bob.Client.AddItemAsync("Eggs");
        await bob.Client.PutAsync($"/api/grocery-items/{bobItem.Id}/basket", content: null);

        var response = await alice.Client.DeleteAsync($"/api/grocery-items/{bobItem.Id}/basket");

        await response.AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        Assert.True(Assert.Single(await bob.Client.GetItemsAsync()).IsInBasket);
    }

    [Fact]
    public async Task AnotherUsersItem_IsIndistinguishableFromANonExistentItem()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        var bobItem = await bob.Client.AddItemAsync("Eggs");

        var foreign = await (await alice.Client.DeleteAsync($"/api/grocery-items/{bobItem.Id}"))
            .AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        var missing = await (await alice.Client.DeleteAsync($"/api/grocery-items/{Guid.NewGuid()}"))
            .AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");

        Assert.Equal(missing, foreign);
    }
}
