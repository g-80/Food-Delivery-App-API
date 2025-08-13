using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.UsersTests;

public class LogInUserHandlerTests
{
    private readonly LogInUserHandler _handler;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public LogInUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new LogInUserHandler(_userRepositoryMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthTokenResponse_WhenLoginIsSuccessful()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "The",
            Surname = "User",
            PhoneNumber = "07123456789",
            Password = new PasswordHasher<User>().HashPassword(null!, "password123"),
            UserType = UserTypes.customer,
        };
        var command = new LogInUserCommand
        {
            PhoneNumber = "07123456789",
            Password = "password123",
        };
        _userRepositoryMock
            .Setup(repo => repo.GetUserByPhoneNumber(command.PhoneNumber))
            .ReturnsAsync(user);
        _tokenServiceMock
            .Setup(service => service.GenerateTokens(user.Id.ToString(), user.UserType.ToString()))
            .ReturnsAsync(
                new AuthTokenResponse
                {
                    AccessToken = "testAccessToken",
                    RefreshToken = "testRefreshToken",
                }
            );

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("testAccessToken");
        result.RefreshToken.Should().Be("testRefreshToken");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var command = new LogInUserCommand
        {
            PhoneNumber = "07123456789",
            Password = "password123",
        };
        _userRepositoryMock
            .Setup(repo => repo.GetUserByPhoneNumber(command.PhoneNumber))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "The",
            Surname = "User",
            PhoneNumber = "07123456789",
            Password = new PasswordHasher<User>().HashPassword(null!, "password123"),
            UserType = UserTypes.customer,
        };
        var command = new LogInUserCommand
        {
            PhoneNumber = "07123456789",
            Password = "wrongPassword",
        };
        _userRepositoryMock
            .Setup(repo => repo.GetUserByPhoneNumber(command.PhoneNumber))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().BeNull();
    }
}
