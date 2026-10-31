using GroceryList.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GroceryList.Api.Errors;

/// <summary>
/// The single place where business errors become HTTP responses.
/// </summary>
public static class ResultProblemExtensions
{
    public const string CodeExtension = "code";

    /// <summary>
    /// Builds a ProblemDetails response for <paramref name="error"/>, with its code in the "code" extension.
    /// </summary>
    public static ObjectResult Problem(this ControllerBase controller, Error error)
    {
        var result = controller.Problem(detail: error.Message, statusCode: ToStatusCode(error.Type));

        if (result.Value is ProblemDetails problem)
        {
            problem.Extensions[CodeExtension] = error.Code;
        }

        return result;
    }

    public static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unmapped error type."),
    };
}
