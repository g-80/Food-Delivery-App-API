using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class CreateOrderHandlerTests
{
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IOrderRepository> _ordersRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _ordersRepositoryMock = new Mock<IOrderRepository>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();
        _handler = new CreateOrderHandler(
            _addressRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _ordersRepositoryMock.Object,
            _paymentServiceMock.Object,
            _loggerMock.Object
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
            },
            Items = new List<CartItem>
            {
                new CartItem
                {
                    CartId = 1,
                    ItemId = 1,
                    Quantity = 2,
                    UnitPrice = 50,
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

        var clientSecret = "test_client_secret";
        _paymentServiceMock
            .Setup(x =>
                x.CreatePaymentIntent(It.Is<Order>(o => o.Id == orderId), It.IsAny<Address>())
            )
            .Returns(new Stripe.PaymentIntent { ClientSecret = clientSecret });

        // Act
        var result = await _handler.Handle(customerId, command);

        // Assert
        result.Should().BeOfType<CreateOrderDTO>();
        result.OrderId.Should().Be(orderId);
        result.ClientSecret.Should().Be(clientSecret);

        _addressRepositoryMock.Verify(
            repo =>
                repo.AddAddress(
                    It.Is<Address>(a =>
                        a.NumberAndStreet == command.DeliveryAddress.NumberAndStreet
                    ),
                    customerId
                ),
            Times.Once
        );
        _cartRepositoryMock.Verify(repo => repo.GetCartByCustomerId(customerId), Times.Once);
        _cartRepositoryMock.Verify(repo => repo.UpdateCart(It.IsAny<Cart>()), Times.Once);
        _ordersRepositoryMock.Verify(repo => repo.AddOrder(It.IsAny<Order>()), Times.Once);
        _ordersRepositoryMock.Verify(
            repo => repo.AddPayment(orderId, It.IsAny<Payment>()),
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
            },
            Items = new List<CartItem>(),
            FoodPlaceId = 1,
        };
        _cartRepositoryMock
            .Setup(repo => repo.GetCartByCustomerId(customerId))
            .ReturnsAsync(emptyCart);

        // Act
        Func<Task> act = () => _handler.Handle(customerId, command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _cartRepositoryMock.Verify(repo => repo.GetCartByCustomerId(customerId), Times.Once);
        _cartRepositoryMock.Verify(repo => repo.UpdateCart(It.IsAny<Cart>()), Times.Never);
        _ordersRepositoryMock.Verify(repo => repo.AddOrder(It.IsAny<Order>()), Times.Never);
        _addressRepositoryMock.Verify(
            repo => repo.AddAddress(It.IsAny<Address>(), customerId),
            Times.Never
        );
        _paymentServiceMock.Verify(
            x => x.CreatePaymentIntent(It.IsAny<Order>(), It.IsAny<Address>()),
            Times.Never
        );
    }
}
