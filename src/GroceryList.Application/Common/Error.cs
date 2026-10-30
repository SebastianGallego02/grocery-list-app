namespace GroceryList.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}

/// <summary>
/// An expected business error. <see cref="Code"/> is stable and machine-readable;
/// <see cref="Message"/> is safe to show to the client.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}
