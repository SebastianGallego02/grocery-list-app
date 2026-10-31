using System.Net;
using System.Net.Http.Json;
using GroceryList.Application.Auth;
using GroceryList.Application.GroceryItems;
using GroceryList.IntegrationTests.Fixtures;

namespace GroceryList.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class GroceryItemsTests(GroceryListApiFactory factory)
{
    [Fact]
    public async Task HappyPath_RegisterLoginAddListTagUntagRemove()
    {
        var anonymous = factory.CreateClient();
        var username = ApiTestHelpers.NewUsername();

        // Register, then log in with the same credentials.
        var register = await anonymous.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            fullName = "Happy Path",
            password = ApiTestHelpers.DefaultPassword,
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await anonymous.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password = ApiTestHelpers.DefaultPassword,
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<AuthResponse>())!.Token;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // Add.
        var add = await client.PostAsJsonAsync("/api/grocery-items", new { name = "Milk", quantity = 2 });
        Assert.Equal(HttpStatusCode.Created, add.StatusCode);
        var item = (await add.Content.ReadFromJsonAsync<GroceryItemResponse>())!;
        Assert.Equal($"/api/grocery-items/{item.Id}", add.Headers.Location?.OriginalString);
        Assert.Equal("Milk", item.Name);
        Assert.Equal(2, item.Quantity);
        Assert.False(item.IsInBasket);

        // List.
        Assert.Equal([item], await client.GetItemsAsync());

        // Tag (put in basket).
        var tag = await client.PutAsync($"/api/grocery-items/{item.Id}/basket", content: null);
        Assert.Equal(HttpStatusCode.NoContent, tag.StatusCode);
        Assert.True(Assert.Single(await client.GetItemsAsync()).IsInBasket);

        // Un-tag (put back).
        var untag = await client.DeleteAsync($"/api/grocery-items/{item.Id}/basket");
        Assert.Equal(HttpStatusCode.NoContent, untag.StatusCode);
        Assert.False(Assert.Single(await client.GetItemsAsync()).IsInBasket);

        // Remove.
        var remove = await client.DeleteAsync($"/api/grocery-items/{item.Id}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
        Assert.Empty(await client.GetItemsAsync());
    }

    [Fact]
    public async Task Add_WithoutQuantity_DefaultsToOne()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.PostAsJsonAsync("/api/grocery-items", new { name = "Bread" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(1, (await response.Content.ReadFromJsonAsync<GroceryItemResponse>())!.Quantity);
    }

    [Fact]
    public async Task Add_DuplicateName_Returns409()
    {
        var user = await factory.RegisterUserAsync();
        await user.Client.AddItemAsync("Milk");

        var response = await user.Client.PostAsJsonAsync("/api/grocery-items", new { name = "Milk" });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "GroceryItems.DuplicateName");
    }

    [Theory]
    [InlineData("milk")]
    [InlineData("MILK")]
    [InlineData("  Milk  ")]
    public async Task Add_DuplicateNameIgnoringCaseAndWhitespace_Returns409(string duplicate)
    {
        var user = await factory.RegisterUserAsync();
        await user.Client.AddItemAsync("Milk");

        var response = await user.Client.PostAsJsonAsync("/api/grocery-items", new { name = duplicate });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "GroceryItems.DuplicateName");
        Assert.Single(await user.Client.GetItemsAsync());
    }

    [Fact]
    public async Task Add_SameNameAsAnotherUser_IsAllowed()
    {
        var alice = await factory.RegisterUserAsync();
        var bob = await factory.RegisterUserAsync();
        await alice.Client.AddItemAsync("Milk");

        var response = await bob.Client.PostAsJsonAsync("/api/grocery-items", new { name = "Milk" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("""{ "name": "" }""", "GroceryItems.NameRequired")]
    [InlineData("""{ "quantity": 2 }""", "GroceryItems.NameRequired")]
    [InlineData("""{ "name": "Milk", "quantity": 0 }""", "GroceryItems.QuantityInvalid")]
    public async Task Add_WithInvalidInput_Returns400ProblemDetails(string json, string expectedCode)
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.PostAsync(
            "/api/grocery-items",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, expectedCode);
    }

    [Fact]
    public async Task Add_WithNameTooLong_Returns400()
    {
        var user = await factory.RegisterUserAsync();

        var response = await user.Client.PostAsJsonAsync(
            "/api/grocery-items",
            new { name = new string('a', GroceryList.Domain.Entities.GroceryItem.NameMaxLength + 1) });

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "GroceryItems.NameTooLong");
    }

    [Fact]
    public async Task Add_LocationHeader_PointsToTheCreatedItem()
    {
        var user = await factory.RegisterUserAsync();
        var add = await user.Client.PostAsJsonAsync("/api/grocery-items", new AddGroceryItemRequest("Milk", 2));
        var created = await add.Content.ReadFromJsonAsync<GroceryItemResponse>();

        var response = await user.Client.GetAsync(add.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created, await response.Content.ReadFromJsonAsync<GroceryItemResponse>());
    }

    [Fact]
    public async Task OperationsOnUnknownItem_Return404()
    {
        var user = await factory.RegisterUserAsync();
        var unknownId = Guid.NewGuid();

        await (await user.Client.GetAsync($"/api/grocery-items/{unknownId}"))
            .AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        await (await user.Client.DeleteAsync($"/api/grocery-items/{unknownId}"))
            .AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        await (await user.Client.PutAsync($"/api/grocery-items/{unknownId}/basket", content: null))
            .AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
        await (await user.Client.DeleteAsync($"/api/grocery-items/{unknownId}/basket"))
            .AssertProblemAsync(HttpStatusCode.NotFound, "GroceryItems.NotFound");
    }
}
