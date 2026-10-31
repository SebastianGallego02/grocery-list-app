using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace GroceryList.IntegrationTests.Fixtures;

/// <summary>
/// Hosts the real API in memory against a real SQL Server running in a Docker container.
/// One container is shared by every test (see <see cref="ApiCollection"/>); tests stay independent
/// by registering their own unique users instead of resetting the database.
/// </summary>
public sealed class GroceryListApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Same image as docker-compose.yml, so a local pull is reused.
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

    public const string JwtSigningKey = "integration-tests-signing-key-0123456789abcdef";

    private MsSqlContainer? _container;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            _container = new MsSqlBuilder(SqlServerImage).Build();
            await _container.StartAsync();
        }
        catch (DockerUnavailableException exception)
        {
            throw new InvalidOperationException(
                "Integration tests run SQL Server in Docker, but Docker is not available. "
                + "Start Docker Desktop and run the tests again.",
                exception);
        }

        _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "GroceryList",
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development: no appsettings.Development.json, so everything below is explicit.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:GroceryListDb", _connectionString);
        builder.UseSetting("Jwt:SigningKey", JwtSigningKey);
        // Also exercises the InitialCreate migration against SQL Server.
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<GroceryListApiFactory>
{
    public const string Name = "Api";
}
