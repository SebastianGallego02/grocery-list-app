using GroceryList.Application.Common;

namespace GroceryList.UnitTests.Application.Common;

public sealed class ResultTests
{
    private static readonly Error SomeError = Error.Validation("Test.Error", "Something is wrong.");

    [Fact]
    public void Success_IsSuccessWithoutError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_IsFailureWithError()
    {
        var result = Result.Failure(SomeError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(SomeError, result.Error);
    }

    [Fact]
    public void Failure_WithNullError_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFailure_AccessingValue_Throws()
    {
        var result = Result.Failure<int>(SomeError);

        Assert.True(result.IsFailure);
        Assert.Equal(SomeError, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<string> result = "value";

        Assert.True(result.IsSuccess);
        Assert.Equal("value", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result<string> generic = SomeError;
        Result plain = SomeError;

        Assert.Equal(SomeError, generic.Error);
        Assert.Equal(SomeError, plain.Error);
    }

    [Theory]
    [MemberData(nameof(ErrorFactories))]
    public void ErrorFactories_SetExpectedType(Error error, ErrorType expectedType)
    {
        Assert.Equal(expectedType, error.Type);
    }

    public static TheoryData<Error, ErrorType> ErrorFactories() => new()
    {
        { Error.Validation("c", "m"), ErrorType.Validation },
        { Error.NotFound("c", "m"), ErrorType.NotFound },
        { Error.Conflict("c", "m"), ErrorType.Conflict },
        { Error.Unauthorized("c", "m"), ErrorType.Unauthorized },
    };

    [Fact]
    public void CatalogueErrors_HaveExpectedTypes()
    {
        Assert.Equal(ErrorType.Conflict, Errors.Auth.UsernameTaken.Type);
        Assert.Equal(ErrorType.Unauthorized, Errors.Auth.InvalidCredentials.Type);
        Assert.Equal(ErrorType.NotFound, Errors.GroceryItems.NotFound.Type);
        Assert.Equal(ErrorType.Conflict, Errors.GroceryItems.DuplicateName.Type);
    }
}
