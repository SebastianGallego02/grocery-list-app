using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GroceryList.IntegrationTests.Fixtures;

namespace GroceryList.IntegrationTests;

/// <summary>
/// Login is mandatory: without a valid token no grocery data is reachable.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class AuthorizationTests(GroceryListApiFactory factory)
{
    public static TheoryData<string, string> ProtectedEndpoints()
    {
        var id = Guid.NewGuid();
        return new TheoryData<string, string>
        {
            { "GET", "/api/users/me" },
            { "GET", "/api/grocery-items" },
            { "POST", "/api/grocery-items" },
            { "DELETE", $"/api/grocery-items/{id}" },
            { "PUT", $"/api/grocery-items/{id}/basket" },
            { "DELETE", $"/api/grocery-items/{id}/basket" },
        };
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task ProtectedEndpoint_WithoutToken_Returns401ProblemDetails(string method, string url)
    {
        var response = await factory.CreateClient().SendAsync(CreateRequest(method, url));

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "Auth.Unauthenticated");
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401(string method, string url)
    {
        var request = CreateRequest(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_IsAnonymousAndReportsHealthyDatabase()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    private static HttpRequestMessage CreateRequest(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST")
        {
            request.Content = JsonContent.Create(new { name = "Milk", quantity = 1 });
        }

        return request;
    }
}
