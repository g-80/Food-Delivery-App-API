using FluentAssertions;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFoodPlaceRepository> _foodPlaceRepositoryMock;
    private readonly CancelOrderHandler _handler;
    private readonly User _user = new User
    {
        Id = 1,
        FirstName = "Test",
        Surname = "User",
        PhoneNumber = "07123456789",
        Password = "hashed_password",
        UserType = UserTypes.customer,
    };

    public CancelOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _foodPlaceRepositoryMock = new Mock<IFoodPlaceRepository>();
        _handler = new CancelOrderHandler(
            _orderRepositoryMock.Object,
            _userRepositoryMock.Object,
            _foodPlaceRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCancelWhenOrderIsPending()
    {
        // Arrange
        var command = new CancelOrderCommand() { Reason = "Doing Tests" };
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            CustomerId = _user.Id,
            FoodPlaceId = 1,
            DeliveryAddressId = 1,
            Subtotal = 100,
            ServiceFee = 10,
            DeliveryFee = 5,
            Total = 115,
            Status = OrderStatuses.pendingConfirmation,
            CreatedAt = DateTime.UtcNow,
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(orderId)).ReturnsAsync(order);
        _userRepositoryMock.Setup(repo => repo.GetUserById(_user.Id)).ReturnsAsync(_user);
        _orderRepositoryMock
            .Setup(repo => repo.UpdateOrderStatus(It.IsAny<Order>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, _user.Id, orderId);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatuses.cancelled);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatus(order), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderIsNotPending()
    {
        // Arrange
        var command = new CancelOrderCommand() { Reason = "Doing Tests" };
        var userId = 1;
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            CustomerId = userId,
            FoodPlaceId = 1,
            DeliveryAddressId = 1,
            Subtotal = 100,
            ServiceFee = 10,
            DeliveryFee = 5,
            Total = 115,
            Status = OrderStatuses.delivering,
            CreatedAt = DateTime.UtcNow,
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(orderId)).ReturnsAsync(order);
        _userRepositoryMock.Setup(repo => repo.GetUserById(userId)).ReturnsAsync(_user);

        // Act
        var result = await _handler.Handle(command, userId, orderId);

        // Assert
        result.Should().BeFalse();
        order.Status.Should().Be(OrderStatuses.delivering);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatus(It.IsAny<Order>()), Times.Never);
    }
}
