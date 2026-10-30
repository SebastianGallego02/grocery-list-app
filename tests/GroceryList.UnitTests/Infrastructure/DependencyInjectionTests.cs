using GroceryList.Application.Abstractions;
using GroceryList.Infrastructure;
using GroceryList.Infrastructure.Persistence;
using GroceryList.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GroceryList.UnitTests.Infrastructure;

public sealed class DependencyInjectionTests
{
    private static readonly Dictionary<string, string?> ValidSettings = new()
    {
        ["ConnectionStrings:GroceryListDb"] = "Server=localhost;Database=GroceryList;Trusted_Connection=True",
        ["Jwt:Issuer"] = "issuer",
        ["Jwt:Audience"] = "audience",
        ["Jwt:SigningKey"] = new string('k', JwtOptions.SigningKeyMinLength),
        ["Jwt:ExpirationMinutes"] = "45",
    };

    [Fact]
    public void AddInfrastructure_RegistersAllPorts()
    {
        using var provider = BuildProvider(ValidSettings);
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<IUserRepository>());
        Assert.NotNull(services.GetRequiredService<IGroceryItemRepository>());
        Assert.NotNull(services.GetRequiredService<IPasswordHasher>());
        Assert.NotNull(services.GetRequiredService<ITokenService>());
        Assert.NotNull(services.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void AddInfrastructure_UnitOfWorkIsTheScopedDbContext()
    {
        using var provider = BuildProvider(ValidSettings);
        using var scope = provider.CreateScope();

        Assert.Same(
            scope.ServiceProvider.GetRequiredService<AppDbContext>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }

    [Fact]
    public void AddInfrastructure_BindsJwtOptions()
    {
        using var provider = BuildProvider(ValidSettings);

        var options = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        Assert.Equal("issuer", options.Issuer);
        Assert.Equal(45, options.ExpirationMinutes);
    }

    [Fact]
    public void AddInfrastructure_WithShortSigningKey_FailsOptionsValidation()
    {
        var settings = new Dictionary<string, string?>(ValidSettings)
        {
            ["Jwt:SigningKey"] = "too-short",
        };
        using var provider = BuildProvider(settings);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);
    }

    [Fact]
    public void AddInfrastructure_WithoutConnectionString_Throws()
    {
        var settings = new Dictionary<string, string?>(ValidSettings);
        settings.Remove("ConnectionStrings:GroceryListDb");

        Assert.Throws<InvalidOperationException>(() => BuildProvider(settings));
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddInfrastructure(configuration).BuildServiceProvider();
    }
}
