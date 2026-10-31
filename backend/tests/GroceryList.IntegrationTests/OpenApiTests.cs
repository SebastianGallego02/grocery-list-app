using System.Net;
using System.Text.Json;
using GroceryList.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;

namespace GroceryList.IntegrationTests;

/// <summary>
/// The OpenAPI document and the Scalar page are off by default and served anonymously when enabled.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class OpenApiTests(GroceryListApiFactory factory)
{
    private HttpClient CreateClientWithOpenApi() =>
        factory.WithWebHostBuilder(builder => builder.UseSetting("OpenApi:Enabled", "true")).CreateClient();

    [Fact]
    public async Task OpenApiDocument_DescribesTheApiAndHowToAuthenticate()
    {
        var response = await CreateClientWithOpenApi().GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var info = document.RootElement.GetProperty("info");
        Assert.Equal("Grocery List API", info.GetProperty("title").GetString());
        var description = info.GetProperty("description").GetString();
        Assert.Contains("POST /api/auth/register", description);
        Assert.Contains("POST /api/auth/login", description);
        Assert.True(document.RootElement.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
    }

    [Fact]
    public async Task ScalarPage_HidesPromotionalFeaturesAndDisablesTelemetry()
    {
        var response = await CreateClientWithOpenApi().GetAsync("/scalar/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<title>Grocery List API</title>", html);
        Assert.Contains("\"showDeveloperTools\":\"never\"", html);
        Assert.Contains("\"agent\":{\"disabled\":true}", html);
        Assert.Contains("\"mcp\":{\"disabled\":true}", html);
        Assert.Contains("\"telemetry\":false", html);
    }

    [Fact]
    public async Task OpenApiDocument_IsNotServedByDefault()
    {
        var response = await factory.CreateClient().GetAsync("/openapi/v1.json");

        // Not mapped: the fallback policy challenges the anonymous caller before routing can 404.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
