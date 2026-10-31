using GroceryList.Application.Abstractions;
using GroceryList.Application.Common;
using GroceryList.Application.Users;
using GroceryList.Domain.Entities;
using NSubstitute;

namespace GroceryList.UnitTests.Application.Users;

public sealed class UserServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository, _currentUser);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsTheUserFromTheToken()
    {
        var user = User.Create("alice", "Alice Smith", "hash", Now);
        _currentUser.UserId.Returns(user.Id);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.GetCurrentUserAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new UserResponse(user.Id, "alice", "Alice Smith"), result.Value);
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserNoLongerExists_ReturnsUnauthorized()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());

        var result = await _sut.GetCurrentUserAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Users.CurrentUserNotFound, result.Error);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task GetCurrentUser_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);

        await _sut.GetCurrentUserAsync(cts.Token);

        await _userRepository.Received(1).GetByIdAsync(userId, cts.Token);
    }
}
