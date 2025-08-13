using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.UsersTests;

public class SignUpUserHandlerTests
{
    private readonly SignUpUserHandler _handler;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ILogger<SignUpUserHandler>> _loggerMock;

    public SignUpUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _loggerMock = new Mock<ILogger<SignUpUserHandler>>();
        _handler = new SignUpUserHandler(
            _userRepositoryMock.Object,
            _addressRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserAlreadyExists()
    {
        // Arrange
        var command = new SignUpUserCommand
        {
            FirstName = "The",
            Surname = "User",
            PhoneNumber = "07123456789",
            Password = "password123",
            UserType = UserTypes.customer,
            Address = new AddressCreateRequest
            {
                NumberAndStreet = "123 Main St",
                City = "Test City",
                Postcode = "W8 8BB",
            },
        };

        _userRepositoryMock
            .Setup(repo => repo.GetUserByPhoneNumber(command.PhoneNumber))
            .ReturnsAsync(
                new User
                {
                    Id = 1,
                    FirstName = "The",
                    Surname = "Existing User",
                    PhoneNumber = command.PhoneNumber,
                    UserType = UserTypes.customer,
                    Password = "hashed_password",
                }
            );

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndAddress_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new SignUpUserCommand
        {
            FirstName = "New",
            Surname = "User",
            PhoneNumber = "07123456789",
            Password = "password123",
            UserType = UserTypes.customer,
            Address = new AddressCreateRequest
            {
                NumberAndStreet = "456 Elm St",
                City = "Test City",
                Postcode = "W8 8BB",
            },
        };

        _userRepositoryMock
            .Setup(repo => repo.GetUserByPhoneNumber(command.PhoneNumber))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(repo => repo.AddUser(It.IsAny<User>())).ReturnsAsync(1);

        _addressRepositoryMock
            .Setup(repo => repo.AddAddress(It.IsAny<Address>(), It.IsAny<int>()))
            .ReturnsAsync(1);

        _cartRepositoryMock
            .Setup(repo => repo.AddCart(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().Be(1);
        _userRepositoryMock.Verify(repo => repo.AddUser(It.IsAny<User>()), Times.Once);
        _addressRepositoryMock.Verify(
            repo => repo.AddAddress(It.IsAny<Address>(), It.IsAny<int>()),
            Times.Once
        );
        _cartRepositoryMock.Verify(repo => repo.AddCart(1), Times.Once);
    }
}
