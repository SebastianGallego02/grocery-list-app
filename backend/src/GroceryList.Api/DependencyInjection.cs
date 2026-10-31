using GroceryList.Api.Auth;
using GroceryList.Api.Errors;
using GroceryList.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GroceryList.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                // Null or missing fields reach the Application validation (which returns error codes)
                // instead of being rejected by MVC's implicit [Required] on non-nullable properties.
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Malformed JSON and similar binding failures: keep MVC's ValidationProblemDetails, add a code.
                options.InvalidModelStateResponseFactory = context =>
                {
                    var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
                    problem.Extensions[ResultProblemExtensions.CodeExtension] = "Request.Invalid";

                    return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
                };
            });

        services.AddProblemDetails(options =>
        {
            // Responses produced by the framework (401 challenge, 404 unknown route...) carry a code too.
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions.TryAdd(
                    ResultProblemExtensions.CodeExtension,
                    context.ProblemDetails.Status switch
                    {
                        StatusCodes.Status401Unauthorized => "Auth.Unauthenticated",
                        StatusCodes.Status403Forbidden => "Auth.Forbidden",
                        StatusCodes.Status404NotFound => "Http.NotFound",
                        StatusCodes.Status405MethodNotAllowed => "Http.MethodNotAllowed",
                        _ => "Http.Error",
                    });
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        return services;
    }
}
