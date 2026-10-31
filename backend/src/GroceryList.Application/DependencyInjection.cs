using GroceryList.Application.Auth;
using GroceryList.Application.GroceryItems;
using GroceryList.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GroceryList.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<AuthService>();
        services.AddScoped<GroceryItemService>();
        services.AddScoped<UserService>();

        return services;
    }
}
