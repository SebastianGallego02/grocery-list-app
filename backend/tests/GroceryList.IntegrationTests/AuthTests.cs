using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using GroceryList.Application.Auth;
using GroceryList.Application.Users;
using GroceryList.IntegrationTests.Fixtures;

namespace GroceryList.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthTests(GroceryListApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ReturnsCreatedWithTokenAndProfile()
    {
        var username = ApiTestHelpers.NewUsername();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            fullName = "Alice Smith",
            password = ApiTestHelpers.DefaultPassword,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/users/me", response.Headers.Location?.OriginalString);

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.Equal(username, auth.Username);
        Assert.Equal("Alice Smith", auth.FullName);
        Assert.True(auth.ExpiresAtUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Register_WithTakenUsername_Returns409()
    {
        var existing = await factory.RegisterUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = existing.Auth.Username,
            fullName = "Someone Else",
            password = ApiTestHelpers.DefaultPassword,
        });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "Auth.UsernameTaken");
    }

    [Fact]
    public async Task Register_WithTakenUsernameInDifferentCase_Returns409()
    {
        var existing = await factory.RegisterUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = existing.Auth.Username.ToUpperInvariant(),
            fullName = "Someone Else",
            password = ApiTestHelpers.DefaultPassword,
        });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "Auth.UsernameTaken");
    }

    [Theory]
    [InlineData("""{ "fullName": "Alice", "password": "Sup3rSecret!" }""", "Auth.UsernameRequired")]
    [InlineData("""{ "username": "ab", "fullName": "Alice", "password": "Sup3rSecret!" }""", "Auth.UsernameLength")]
    [InlineData("""{ "username": "alice_valid", "password": "Sup3rSecret!" }""", "Auth.FullNameRequired")]
    [InlineData("""{ "username": "alice_valid", "fullName": "Alice", "password": "short" }""", "Auth.PasswordLength")]
    public async Task Register_WithInvalidInput_Returns400ProblemDetails(string json, string expectedCode)
    {
        var response = await _client.PostAsync(
            "/api/auth/register",
            new StringContent(json, Encoding.UTF8, "application/json"));

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, expectedCode);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }

    [Fact]
    public async Task Register_WithMalformedJson_Returns400ProblemDetails()
    {
        var response = await _client.PostAsync(
            "/api/auth/register",
            new StringContent("{ not json", Encoding.UTF8, "application/json"));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "Request.Invalid");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var user = await factory.RegisterUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = user.Auth.Username,
            password = user.Password,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        Assert.Equal(user.Auth.Username, auth.Username);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownUser_Returns401WithTheSameError()
    {
        var user = await factory.RegisterUserAsync();

        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = user.Auth.Username,
            password = "Wrong-Passw0rd",
        });
        var unknownUser = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = ApiTestHelpers.NewUsername(),
            password = user.Password,
        });

        var first = await wrongPassword.AssertProblemAsync(HttpStatusCode.Unauthorized, "Auth.InvalidCredentials");
        var second = await unknownUser.AssertProblemAsync(HttpStatusCode.Unauthorized, "Auth.InvalidCredentials");
        Assert.Equal(first.Detail, second.Detail);
    }

    [Fact]
    public async Task Login_WithMissingPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "alice" });

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "Auth.PasswordRequired");
    }

    [Fact]
    public async Task Me_ReturnsTheUserFromTheToken()
    {
        var user = await factory.RegisterUserAsync();

        var me = await user.Client.GetFromJsonAsync<UserResponse>("/api/users/me");

        Assert.NotNull(me);
        Assert.Equal(user.Auth.Username, me.Username);
        Assert.Equal(user.Auth.FullName, me.FullName);
    }

    [Fact]
    public async Task TokenFromLogin_GrantsAccessToProtectedEndpoints()
    {
        var user = await factory.RegisterUserAsync();
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = user.Auth.Username,
            password = user.Password,
        });
        var token = (await login.Content.ReadFromJsonAsync<AuthResponse>())!.Token;

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
