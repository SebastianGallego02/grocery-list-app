using GroceryList.Api;
using GroceryList.Application;
using GroceryList.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

var app = builder.Build();

app.UseExceptionHandler();
// Turns empty error responses (e.g. 404 for unknown routes) into ProblemDetails.
app.UseStatusCodePages();

app.MapControllers();

app.Run();

// Exposes the entry point to WebApplicationFactory in the integration tests.
public partial class Program;
