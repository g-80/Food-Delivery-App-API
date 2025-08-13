using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class CreateOrderHandlerTests
{
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IOrderRepository> _ordersRepositoryMock;
    private readonly Mock<IOrderConfirmationService> _orderConfirmationServiceMock;
    private readonly Mock<IDeliveryAssignmentService> _deliveryAssignmentServiceMock;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _ordersRepositoryMock = new Mock<IOrderRepository>();
        _orderConfirmationServiceMock = new Mock<IOrderConfirmationService>();
        _deliveryAssignmentServiceMock = new Mock<IDeliveryAssignmentService>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _handler = new CreateOrderHandler(
            _addressRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _ordersRepositoryMock.Object,
            _orderConfirmationServiceMock.Object,
            _deliveryAssignmentServiceMock.Object,
            _loggerMock.Object,
            _backgroundJobClientMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenCartIsNotEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            DeliveryAddress = new AddressCreateRequest
            {
                NumberAndStreet = "123 Main St",
                City = "Test City",
                Postcode = "12345",
            },
        };
        int customerId = 1;

        var addressId = 1;
        _addressRepositoryMock
            .Setup(repo => repo.AddAddress(It.IsAny<Address>(), customerId))
            .ReturnsAsync(addressId);

        var cart = new Cart
        {
            Id = 1,
            CustomerId = customerId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Pricing = new CartPricing()
            {
                CartId = 1,
                Subtotal = 100,
                ServiceFee = 0,
                DeliveryFee = 0,
                Total = 100,
            },
            Items = new List<CartItem>
            {
                new CartItem
                {
                    CartId = 1,
                    ItemId = 1,
                    Quantity = 2,
                    UnitPrice = 50,
                    Subtotal = 100,
                },
            },
            FoodPlaceId = 1,
        };
        _cartRepositoryMock.Setup(repo => repo.GetCartByCustomerId(customerId)).ReturnsAsync(cart);

        var orderId = 1;
        _ordersRepositoryMock.Setup(repo => repo.AddOrder(It.IsAny<Order>())).ReturnsAsync(orderId);

        _cartRepositoryMock
            .Setup(repo => repo.UpdateCart(It.IsAny<Cart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, customerId);

        // Assert
        result.Should().Be(orderId);
        cart.Items.Should().BeEmpty();
        _addressRepositoryMock.Verify(
            repo => repo.AddAddress(It.IsAny<Address>(), customerId),
            Times.Once
        );
        _cartRepositoryMock.Verify(repo => repo.GetCartByCustomerId(customerId), Times.Once);
        _cartRepositoryMock.Verify(repo => repo.UpdateCart(It.IsAny<Cart>()), Times.Once);
        _ordersRepositoryMock.Verify(repo => repo.AddOrder(It.IsAny<Order>()), Times.Once);
        _backgroundJobClientMock.Verify(
            client =>
                client.Create(
                    It.Is<Job>(job =>
                        job.Method.Name == "ProcessOrderAsync"
                        && (job.Args[0] as Order).Id == orderId
                    ),
                    It.IsAny<EnqueuedState>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCartIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            DeliveryAddress = new AddressCreateRequest
            {
                NumberAndStreet = "123 Main St",
                City = "Test City",
                Postcode = "12345",
            },
        };
        int customerId = 1;

        _addressRepositoryMock
            .Setup(repo => repo.AddAddress(It.IsAny<Address>(), customerId))
            .ReturnsAsync(1);

        var emptyCart = new Cart
        {
            Id = 1,
            CustomerId = customerId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Pricing = new CartPricing()
            {
                CartId = 1,
                Subtotal = 0,
                ServiceFee = 0,
                DeliveryFee = 0,
                Total = 0,
            },
            Items = new List<CartItem>(),
            FoodPlaceId = 1,
        };
        _cartRepositoryMock
            .Setup(repo => repo.GetCartByCustomerId(customerId))
            .ReturnsAsync(emptyCart);

        // Act
        Func<Task> act = () => _handler.Handle(command, customerId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _cartRepositoryMock.Verify(repo => repo.GetCartByCustomerId(customerId), Times.Once);
        _ordersRepositoryMock.Verify(repo => repo.AddOrder(It.IsAny<Order>()), Times.Never);
        _addressRepositoryMock.Verify(
            repo => repo.AddAddress(It.IsAny<Address>(), customerId),
            Times.Never
        );
        _cartRepositoryMock.Verify(repo => repo.UpdateCart(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_ShouldCancelOrder_WhenOrderIsRejected()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            FoodPlaceId = 1,
            DeliveryAddressId = 1,
            Subtotal = 100,
            ServiceFee = 10,
            DeliveryFee = 5,
            Total = 115,
            Status = OrderStatuses.pending,
            CreatedAt = DateTime.UtcNow,
        };

        _orderConfirmationServiceMock
            .Setup(service => service.RequestOrderConfirmation(order))
            .ReturnsAsync(false);

        // Act
        await _handler.ProcessOrderAsync(order);

        // Assert
        _orderConfirmationServiceMock.Verify(
            service => service.RequestOrderConfirmation(order),
            Times.Once
        );
        order.Status.Should().Be(OrderStatuses.cancelled);
        _ordersRepositoryMock.Verify(
            repo =>
                repo.UpdateOrderStatus(
                    It.Is<Order>(o => o.Id == order.Id && o.Status == OrderStatuses.cancelled)
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessOrderAsync_ShouldProcessOrder_WhenConfirmed()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            FoodPlaceId = 1,
            DeliveryAddressId = 1,
            Subtotal = 100,
            ServiceFee = 10,
            DeliveryFee = 5,
            Total = 115,
            Status = OrderStatuses.pending,
            CreatedAt = DateTime.UtcNow,
        };

        _orderConfirmationServiceMock
            .Setup(service => service.RequestOrderConfirmation(order))
            .ReturnsAsync(true);

        // Act
        await _handler.ProcessOrderAsync(order);

        // Assert
        _orderConfirmationServiceMock.Verify(
            service => service.RequestOrderConfirmation(order),
            Times.Once
        );
        order.Status.Should().Be(OrderStatuses.preparing);
        order.Delivery.Should().NotBeNull();
        _ordersRepositoryMock.Verify(
            repo =>
                repo.UpdateOrderStatus(
                    It.Is<Order>(o => o.Id == order.Id && o.Status == OrderStatuses.preparing)
                ),
            Times.Once
        );
        _deliveryAssignmentServiceMock.Verify(
            service => service.InitiateDeliveryAssignment(order),
            Times.Once
        );
    }
}
