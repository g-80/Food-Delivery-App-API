using FluentAssertions;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFoodPlaceRepository> _foodPlaceRepositoryMock;
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _foodPlaceRepositoryMock = new Mock<IFoodPlaceRepository>();
        _handler = new UpdateOrderStatusHandler(
            _orderRepositoryMock.Object,
            _foodPlaceRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderNotInRequiredStatus()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand { Status = OrderStatuses.readyForPickup };
        var userId = 1;
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            CustomerId = userId,
            FoodPlaceId = 1,
            DeliveryAddressId = 1,
            ServiceFee = 10,
            DeliveryFee = 5,
            Status = OrderStatuses.pendingConfirmation,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(orderId)).ReturnsAsync(order);
        _foodPlaceRepositoryMock
            .Setup(repo => repo.GetFoodPlaceUserId(order.FoodPlaceId))
            .ReturnsAsync(userId);

        // Act
        Func<Task> act = () => _handler.Handle(command, userId, orderId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateOrderStatus_WhenInRequiredStatus()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand { Status = OrderStatuses.preparing };
        var userId = 1;
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            CustomerId = userId,
            FoodPlaceId = 1,
            DeliveryAddressId = 1,
            ServiceFee = 10,
            DeliveryFee = 5,
            Status = OrderStatuses.pendingConfirmation,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderById(orderId)).ReturnsAsync(order);
        _foodPlaceRepositoryMock
            .Setup(repo => repo.GetFoodPlaceUserId(order.FoodPlaceId))
            .ReturnsAsync(userId);
        _orderRepositoryMock
            .Setup(repo => repo.UpdateOrderStatus(It.IsAny<Order>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, userId, orderId);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatuses.preparing);
        _orderRepositoryMock.Verify(repo => repo.UpdateOrderStatus(order), Times.Once);
    }
}
