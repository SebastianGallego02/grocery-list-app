using GroceryList.Api;
using GroceryList.Api.Database;
using GroceryList.Api.OpenApi;
using GroceryList.Application;
using GroceryList.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsIfEnabledAsync();

app.UseExceptionHandler();
// Turns empty error responses (401 challenge, 404 unknown route) into ProblemDetails.
app.UseStatusCodePages();

// No HTTPS redirection: in containers and behind a reverse proxy TLS terminates in front of the app.
app.UseCors(GroceryList.Api.DependencyInjection.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options => options
        .WithTitle(ApiInfoTransformer.Title)
        .AddPreferredSecuritySchemes(BearerSecurityTransformer.SchemeName)
        // Hide Scalar's promotional features: the developer toolbar ("Deploy", "Generate SDK"),
        // the AI chat ("Ask AI") and the MCP integration.
        .HideDeveloperTools()
        .DisableAgent()
        .DisableMcp()
        // A demo should not send usage data to third parties.
        .DisableTelemetry())
        .AllowAnonymous();
}

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

await app.RunAsync();

// Exposes the entry point to WebApplicationFactory in the integration tests.
public partial class Program;
