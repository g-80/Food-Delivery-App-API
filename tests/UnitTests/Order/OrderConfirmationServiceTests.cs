using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class OrderConfirmationServiceTests
{
    private readonly Mock<IOrdersConfirmations> _mockOrdersConfirmations;
    private readonly Mock<IFoodPlaceRepository> _mockFoodPlaceRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<OrderConfirmationService>> _mockLogger;
    private readonly Mock<IHubContext<FoodPlaceHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly OrderConfirmationService _service;

    public OrderConfirmationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<FoodPlaceHub>>();
        _mockOrdersConfirmations = new Mock<IOrdersConfirmations>();
        _mockFoodPlaceRepository = new Mock<IFoodPlaceRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<OrderConfirmationService>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _service = new OrderConfirmationService(
            _mockHubContext.Object,
            _mockOrdersConfirmations.Object,
            _mockFoodPlaceRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task RequestOrderConfirmation_WhenOrderIsConfirmed_ReturnsTrue()
    {
        // Arrange
        var order = CreateTestOrder();
        var foodPlace = CreateTestFoodPlace();
        var customer = CreateTestUser();
        var foodPlaceUserId = 1;

        _mockFoodPlaceRepository
            .Setup(x => x.GetFoodPlaceUserId(It.IsAny<int>()))
            .ReturnsAsync(foodPlaceUserId);
        _mockFoodPlaceRepository
            .Setup(x => x.GetFoodPlaceById(It.IsAny<int>()))
            .ReturnsAsync(foodPlace);
        _mockUserRepository.Setup(x => x.GetUserById(It.IsAny<int>())).ReturnsAsync(customer);

        var orderConfirmation = new OrderConfirmation
        {
            CancellationTokenSource = new CancellationTokenSource(),
            IsConfirmed = false,
        };

        _mockOrdersConfirmations
            .Setup(x => x.AddOrderConfirmation(order.Id, It.IsAny<OrderConfirmation>()))
            .Callback<int, OrderConfirmation>(
                (id, confirmation) =>
                {
                    // Simulate immediate confirmation
                    confirmation.IsConfirmed = true;
                    confirmation.CancellationTokenSource.Cancel();
                }
            );

        // Act
        var result = await _service.RequestOrderConfirmation(order);

        // Assert
        result.Should().BeTrue();
        _mockClientProxy.Verify(
            x =>
                x.SendCoreAsync(
                    "ReceiveOrderConfirmation",
                    It.Is<object[]>(args => args.Length == 1 && args[0] is OrderConfirmationDTO),
                    default
                ),
            Times.Once
        );
        _mockOrdersConfirmations.Verify(x => x.RemoveOrderConfirmation(order.Id), Times.Once);
    }

    [Fact]
    public async Task RequestOrderConfirmation_WhenOrderTimesOut_ReturnsFalse()
    {
        // Arrange
        var order = CreateTestOrder();
        var foodPlace = CreateTestFoodPlace();
        var customer = CreateTestUser();
        var foodPlaceUserId = 1;

        _mockFoodPlaceRepository
            .Setup(x => x.GetFoodPlaceUserId(It.IsAny<int>()))
            .ReturnsAsync(foodPlaceUserId);
        _mockFoodPlaceRepository
            .Setup(x => x.GetFoodPlaceById(It.IsAny<int>()))
            .ReturnsAsync(foodPlace);
        _mockUserRepository.Setup(x => x.GetUserById(It.IsAny<int>())).ReturnsAsync(customer);

        // Act
        var result = await _service.RequestOrderConfirmation(order);

        // Assert
        result.Should().BeFalse();
        _mockClientProxy.Verify(
            x =>
                x.SendCoreAsync(
                    "ReceiveOrderConfirmation",
                    It.Is<object[]>(args => args.Length == 1 && args[0] is OrderConfirmationDTO),
                    default
                ),
            Times.Once
        );
        _mockOrdersConfirmations.Verify(x => x.RemoveOrderConfirmation(order.Id), Times.Once);
    }

    [Fact]
    public void ConfirmOrder_WhenOrderExists_SetsIsConfirmedToTrueAndCancelsToken()
    {
        // Arrange
        var orderId = 123;
        var cancellationTokenSource = new CancellationTokenSource();
        var orderConfirmation = new OrderConfirmation
        {
            CancellationTokenSource = cancellationTokenSource,
            IsConfirmed = false,
        };

        _mockOrdersConfirmations
            .Setup(x => x.GetOrderConfirmation(orderId))
            .Returns(orderConfirmation);

        // Act
        _service.ConfirmOrder(orderId);

        // Assert
        orderConfirmation.IsConfirmed.Should().BeTrue();
        cancellationTokenSource.Token.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void RejectOrder_WhenOrderExists_CancelsTokenWithoutSettingConfirmation()
    {
        // Arrange
        var orderId = 123;
        var cancellationTokenSource = new CancellationTokenSource();
        var orderConfirmation = new OrderConfirmation
        {
            CancellationTokenSource = cancellationTokenSource,
            IsConfirmed = false,
        };

        _mockOrdersConfirmations
            .Setup(x => x.GetOrderConfirmation(orderId))
            .Returns(orderConfirmation);

        // Act
        _service.RejectOrder(orderId);

        // Assert
        orderConfirmation.IsConfirmed.Should().BeFalse();
        cancellationTokenSource.Token.IsCancellationRequested.Should().BeTrue();
    }

    private Order CreateTestOrder(int id = 1)
    {
        return new Order
        {
            Id = id,
            CustomerId = 1,
            FoodPlaceId = 1,
            DeliveryAddressId = 2,
            Items = new List<OrderItem>(),
            Subtotal = 1000,
            ServiceFee = 0,
            DeliveryFee = 200,
            Total = 1200,
            Delivery = new Delivery
            {
                Id = 1,
                AddressId = 2,
                ConfirmationCode = "Testing",
                Status = DeliveryStatuses.assigningDriver,
            },
            Status = OrderStatuses.preparing,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private FoodPlace CreateTestFoodPlace()
    {
        return new FoodPlace
        {
            Id = 1,
            Name = "Test Restaurant",
            Description = "A place for testing",
            Category = "Test Category",
            AddressId = 1,
            Location = new Location { Latitude = 51.505, Longitude = -0.3099 },
        };
    }

    private User CreateTestUser(int id = 1)
    {
        return new User
        {
            Id = id,
            FirstName = "John",
            Surname = "Doe",
            PhoneNumber = "07123456789",
            Password = "hashed_password",
            UserType = UserTypes.customer,
        };
    }
}
