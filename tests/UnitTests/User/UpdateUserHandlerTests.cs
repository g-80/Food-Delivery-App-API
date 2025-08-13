using FluentAssertions;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.UsersTests;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new UpdateUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            FirstName = "Updated",
            Surname = "User",
            PhoneNumber = "07123456789",
            Password = "newpassword123",
        };
        int userId = 1;
        _userRepositoryMock.Setup(repo => repo.GetUserById(userId)).ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
