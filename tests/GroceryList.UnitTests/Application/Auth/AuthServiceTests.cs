using GroceryList.Application.Abstractions;
using GroceryList.Application.Auth;
using GroceryList.Application.Common;
using GroceryList.Domain.Entities;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GroceryList.UnitTests.Application.Auth;

public sealed class AuthServiceTests
{
    private const string ValidPassword = "s3cretPassword";
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly AccessToken Token = new("jwt-token", Now.AddHours(1));

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        // Fake hasher: "hashed:<password>", so Verify only succeeds for the original password.
        _passwordHasher.Hash(Arg.Any<string>()).Returns(call => Hashed(call.Arg<string>()));
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(1) == Hashed(call.ArgAt<string>(0)));
        _tokenService.CreateToken(Arg.Any<User>()).Returns(Token);

        _sut = new AuthService(_userRepository, _unitOfWork, _passwordHasher, _tokenService, new FakeTimeProvider(Now));
    }

    // ---------- Register ----------

    [Fact]
    public async Task Register_WithValidRequest_CreatesUserAndReturnsToken()
    {
        var result = await _sut.RegisterAsync(
            new RegisterRequest("  alice  ", "  Alice Smith  ", ValidPassword), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthResponse(Token.Token, Token.ExpiresAtUtc, "alice", "Alice Smith"), result.Value);

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u.Username == "alice" &&
                u.FullName == "Alice Smith" &&
                u.PasswordHash == Hashed(ValidPassword) &&
                u.CreatedAtUtc == Now),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_ChecksUniquenessWithTrimmedUsername()
    {
        await _sut.RegisterAsync(new RegisterRequest("  alice ", "Alice", ValidPassword), CancellationToken.None);

        await _userRepository.Received(1).ExistsByUsernameAsync("alice", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        await _sut.RegisterAsync(new RegisterRequest("alice", "Alice", ValidPassword), cts.Token);

        await _userRepository.Received(1).ExistsByUsernameAsync("alice", cts.Token);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), cts.Token);
        await _unitOfWork.Received(1).SaveChangesAsync(cts.Token);
    }

    [Fact]
    public async Task Register_WithTakenUsername_ReturnsConflictAndDoesNotPersist()
    {
        _userRepository.ExistsByUsernameAsync("alice", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.RegisterAsync(
            new RegisterRequest("alice", "Alice", ValidPassword), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Auth.UsernameTaken, result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        await AssertNothingPersistedAsync();
        _tokenService.DidNotReceiveWithAnyArgs().CreateToken(default!);
    }

    [Fact]
    public async Task Register_WhenConcurrentRegistrationWinsTheRace_ReturnsConflict()
    {
        // The uniqueness check passes, but another request inserts the same username before we save.
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new UniqueConstraintViolationException("duplicate"));

        var result = await _sut.RegisterAsync(
            new RegisterRequest("alice", "Alice", ValidPassword), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Auth.UsernameTaken, result.Error);
        _tokenService.DidNotReceiveWithAnyArgs().CreateToken(default!);
    }

    [Theory]
    [MemberData(nameof(InvalidRegisterRequests))]
    public async Task Register_WithInvalidInput_ReturnsValidationErrorWithoutTouchingRepository(
        RegisterRequest request, Error expected)
    {
        var result = await _sut.RegisterAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expected, result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        await _userRepository.DidNotReceiveWithAnyArgs().ExistsByUsernameAsync(default!, default);
        await AssertNothingPersistedAsync();
    }

    public static TheoryData<RegisterRequest, Error> InvalidRegisterRequests() => new()
    {
        { new RegisterRequest(null!, "Alice", ValidPassword), Errors.Auth.UsernameRequired },
        { new RegisterRequest("   ", "Alice", ValidPassword), Errors.Auth.UsernameRequired },
        { new RegisterRequest(new string('a', User.UsernameMinLength - 1), "Alice", ValidPassword), Errors.Auth.UsernameLength },
        { new RegisterRequest(new string('a', User.UsernameMaxLength + 1), "Alice", ValidPassword), Errors.Auth.UsernameLength },
        { new RegisterRequest("alice", null!, ValidPassword), Errors.Auth.FullNameRequired },
        { new RegisterRequest("alice", "  ", ValidPassword), Errors.Auth.FullNameRequired },
        { new RegisterRequest("alice", new string('a', User.FullNameMaxLength + 1), ValidPassword), Errors.Auth.FullNameTooLong },
        { new RegisterRequest("alice", "Alice", null!), Errors.Auth.PasswordRequired },
        { new RegisterRequest("alice", "Alice", ""), Errors.Auth.PasswordRequired },
        { new RegisterRequest("alice", "Alice", new string('p', AuthService.PasswordMinLength - 1)), Errors.Auth.PasswordLength },
        { new RegisterRequest("alice", "Alice", new string('p', AuthService.PasswordMaxLength + 1)), Errors.Auth.PasswordLength },
    };

    [Theory]
    [InlineData(AuthService.PasswordMinLength)]
    [InlineData(AuthService.PasswordMaxLength)]
    public async Task Register_WithPasswordAtBoundaries_Succeeds(int length)
    {
        var result = await _sut.RegisterAsync(
            new RegisterRequest("alice", "Alice", new string('p', length)), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    // ---------- Login ----------

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var user = GivenExistingUser("alice", ValidPassword);

        var result = await _sut.LoginAsync(new LoginRequest(" alice ", ValidPassword), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthResponse(Token.Token, Token.ExpiresAtUtc, user.Username, user.FullName), result.Value);
        _tokenService.Received(1).CreateToken(user);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsInvalidCredentials()
    {
        var result = await _sut.LoginAsync(new LoginRequest("ghost", ValidPassword), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Auth.InvalidCredentials, result.Error);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        _tokenService.DidNotReceiveWithAnyArgs().CreateToken(default!);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsInvalidCredentials()
    {
        GivenExistingUser("alice", ValidPassword);

        var result = await _sut.LoginAsync(new LoginRequest("alice", "wrongPassword"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Auth.InvalidCredentials, result.Error);
        _tokenService.DidNotReceiveWithAnyArgs().CreateToken(default!);
    }

    [Fact]
    public async Task Login_UnknownUserAndWrongPassword_ReturnIndistinguishableErrors()
    {
        GivenExistingUser("alice", ValidPassword);

        var unknownUser = await _sut.LoginAsync(new LoginRequest("ghost", ValidPassword), CancellationToken.None);
        var wrongPassword = await _sut.LoginAsync(new LoginRequest("alice", "wrongPassword"), CancellationToken.None);

        Assert.Equal(unknownUser.Error, wrongPassword.Error);
    }

    [Theory]
    [MemberData(nameof(InvalidLoginRequests))]
    public async Task Login_WithMissingInput_ReturnsValidationErrorWithoutLookup(LoginRequest request, Error expected)
    {
        var result = await _sut.LoginAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expected, result.Error);
        await _userRepository.DidNotReceiveWithAnyArgs().GetByUsernameAsync(default!, default);
    }

    public static TheoryData<LoginRequest, Error> InvalidLoginRequests() => new()
    {
        { new LoginRequest(null!, ValidPassword), Errors.Auth.UsernameRequired },
        { new LoginRequest("  ", ValidPassword), Errors.Auth.UsernameRequired },
        { new LoginRequest("alice", null!), Errors.Auth.PasswordRequired },
        { new LoginRequest("alice", ""), Errors.Auth.PasswordRequired },
    };

    private static string Hashed(string password) => $"hashed:{password}";

    private User GivenExistingUser(string username, string password)
    {
        var user = User.Create(username, "Alice Smith", Hashed(password), Now);
        _userRepository.GetByUsernameAsync(username, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    private async Task AssertNothingPersistedAsync()
    {
        await _userRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
