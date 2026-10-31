using GroceryList.Application.Abstractions;
using GroceryList.Application.Common;
using GroceryList.Domain.Entities;

namespace GroceryList.Application.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    TimeProvider timeProvider)
{
    public const int PasswordMinLength = 8;

    // Upper bound keeps hashing cost predictable for absurdly long inputs.
    public const int PasswordMaxLength = 128;

    // A real hash of a random, discarded password, in the configured hasher's format
    // (ASP.NET Core Identity v3: PBKDF2-HMAC-SHA512, 100,000 iterations). Verifying against it costs
    // the same as verifying a real user's hash. Infrastructure tests check the format stays in sync.
    internal const string DummyPasswordHash =
        "AQAAAAIAAYagAAAAEN4n2EhI/A0HFX9V/ZH4HeA+S1AS+l1n3dX3frkmKavSPnigMyxzHefy87kCJsYJrw==";

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (Validate(request) is { } validationError)
        {
            return validationError;
        }

        var username = request.Username.Trim();

        if (await userRepository.ExistsByUsernameAsync(username, cancellationToken))
        {
            return Errors.Auth.UsernameTaken;
        }

        var user = User.Create(
            username,
            request.FullName,
            passwordHasher.Hash(request.Password),
            timeProvider.GetUtcNow());

        await userRepository.AddAsync(user, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A concurrent registration took the username between the check above and the insert.
            return Errors.Auth.UsernameTaken;
        }

        return user.ToAuthResponse(tokenService.CreateToken(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Only presence is checked here: applying the registration rules would hint at what is wrong.
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Errors.Auth.UsernameRequired;
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return Errors.Auth.PasswordRequired;
        }

        var user = await userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);

        // Same error for unknown user and wrong password, so usernames cannot be enumerated.
        if (user is null)
        {
            // Password hashing is deliberately slow. Skipping it for unknown usernames would make those
            // responses measurably faster and leak which usernames exist, so verify against a dummy hash.
            passwordHasher.Verify(request.Password, DummyPasswordHash);
            return Errors.Auth.InvalidCredentials;
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Errors.Auth.InvalidCredentials;
        }

        return user.ToAuthResponse(tokenService.CreateToken(user));
    }

    private static Error? Validate(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Errors.Auth.UsernameRequired;
        }

        if (request.Username.Trim().Length is < User.UsernameMinLength or > User.UsernameMaxLength)
        {
            return Errors.Auth.UsernameLength;
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Errors.Auth.FullNameRequired;
        }

        if (request.FullName.Trim().Length > User.FullNameMaxLength)
        {
            return Errors.Auth.FullNameTooLong;
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return Errors.Auth.PasswordRequired;
        }

        if (request.Password.Length is < PasswordMinLength or > PasswordMaxLength)
        {
            return Errors.Auth.PasswordLength;
        }

        return null;
    }
}
