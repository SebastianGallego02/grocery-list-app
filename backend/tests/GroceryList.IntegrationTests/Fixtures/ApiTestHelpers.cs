using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GroceryList.Application.Auth;
using GroceryList.Application.GroceryItems;

namespace GroceryList.IntegrationTests.Fixtures;

/// <summary>The fields of a ProblemDetails response that the tests assert on.</summary>
public sealed record ProblemResponse(int Status, string? Title, string? Detail, string? Code);

public sealed record AuthenticatedUser(HttpClient Client, AuthResponse Auth, string Password);

internal static class ApiTestHelpers
{
    public const string DefaultPassword = "Sup3rSecret!";

    public static string NewUsername() => $"u{Guid.NewGuid():N}";

    public static async Task<AuthenticatedUser> RegisterUserAsync(this GroceryListApiFactory factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = NewUsername(),
            fullName = "Test User",
            password = DefaultPassword,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        return new AuthenticatedUser(client, auth, DefaultPassword);
    }

    public static async Task<GroceryItemResponse> AddItemAsync(this HttpClient client, string name, int quantity = 1)
    {
        var response = await client.PostAsJsonAsync("/api/grocery-items", new { name, quantity });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<GroceryItemResponse>())!;
    }

    public static async Task<IReadOnlyList<GroceryItemResponse>> GetItemsAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/grocery-items");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<List<GroceryItemResponse>>())!;
    }

    /// <summary>Asserts a ProblemDetails response with the expected status and error code.</summary>
    public static async Task<ProblemResponse> AssertProblemAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = (await response.Content.ReadFromJsonAsync<ProblemResponse>())!;
        Assert.Equal((int)expectedStatus, problem.Status);
        Assert.Equal(expectedCode, problem.Code);

        return problem;
    }
}
