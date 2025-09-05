using FluentAssertions;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFoodPlaceRepository> _foodPlaceRepositoryMock;
    private readonly Mock<IOrderCancellationService> _orderCancellationServiceMock;
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _foodPlaceRepositoryMock = new Mock<IFoodPlaceRepository>();
        _orderCancellationServiceMock = new Mock<IOrderCancellationService>();
        _handler = new CancelOrderHandler(
            _orderRepositoryMock.Object,
            _userRepositoryMock.Object,
            _foodPlaceRepositoryMock.Object,
            _orderCancellationServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCancelWhenOrderIsPending()
    {
        // Arrange
        var command = new CancelOrderCommand() { Reason = "Doing Tests" };

        var orderId = 1;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        var user = OrderTestsHelper.CreateTestUser();

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(orderId)).ReturnsAsync(order);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        _orderCancellationServiceMock
            .Setup(x => x.CancelOrder(order, command.Reason))
            .Callback<Order, string>(
                (o, reason) =>
                {
                    o.Status = OrderStatuses.cancelled;
                }
            )
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, user.Id, orderId);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatuses.cancelled);
        _orderCancellationServiceMock.Verify(x => x.CancelOrder(order, command.Reason), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderIsNotPending()
    {
        // Arrange
        var command = new CancelOrderCommand() { Reason = "Doing Tests" };

        var user = OrderTestsHelper.CreateTestUser();

        var orderId = 1;
        var order = OrderTestsHelper.CreateTestOrder(orderId);
        order.Status = OrderStatuses.preparing;
        order.Delivery!.Status = DeliveryStatuses.pickup;

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(orderId)).ReturnsAsync(order);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, user.Id, orderId);

        // Assert
        result.Should().BeFalse();
        order.Status.Should().Be(OrderStatuses.preparing);
        _orderCancellationServiceMock.Verify(
            x => x.CancelOrder(It.IsAny<Order>(), It.IsAny<string>()),
            Times.Never
        );
    }
}
