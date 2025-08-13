using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class DeliveryAssignmentServiceTests
{
    private readonly Mock<IDriverRepository> _mockDriverRepository;
    private readonly Mock<IFoodPlaceRepository> _mockFoodPlaceRepository;
    private readonly Mock<IAddressRepository> _mockAddressRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<JourneyCalculationService> _mockJourneyCalculationService;
    private readonly Mock<IHubContext<DriverHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IDeliveriesAssignments> _mockDeliveriesAssignments;
    private readonly Mock<ILogger<DeliveryAssignmentService>> _mockLogger;
    private readonly DeliveryAssignmentService _service;

    public DeliveryAssignmentServiceTests()
    {
        _mockDriverRepository = new Mock<IDriverRepository>();
        _mockFoodPlaceRepository = new Mock<IFoodPlaceRepository>();
        _mockAddressRepository = new Mock<IAddressRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockJourneyCalculationService = new Mock<JourneyCalculationService>();
        _mockHubContext = new Mock<IHubContext<DriverHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockDeliveriesAssignments = new Mock<IDeliveriesAssignments>();
        _mockLogger = new Mock<ILogger<DeliveryAssignmentService>>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _service = new DeliveryAssignmentService(
            _mockDriverRepository.Object,
            _mockFoodPlaceRepository.Object,
            _mockAddressRepository.Object,
            _mockJourneyCalculationService.Object,
            _mockOrderRepository.Object,
            _mockHubContext.Object,
            _mockDeliveriesAssignments.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task InitiateDeliveryAssignment_WithAvailableDrivers_ShouldSendOffersToAllDrivers()
    {
        // Arrange
        var order = CreateTestOrder();
        var foodPlace = CreateTestFoodPlace();
        var drivers = CreateTestAvailableDrivers(2);
        var job = CreateTestDeliveryAssignmentJob(order.Id);
        var addresses = CreateTestAddresses();

        _mockDeliveriesAssignments.Setup(x => x.GetOrCreateAssignmentJob(order.Id)).Returns(job);
        _mockFoodPlaceRepository
            .Setup(x => x.GetFoodPlaceById(order.FoodPlaceId))
            .ReturnsAsync(foodPlace);
        _mockDriverRepository
            .Setup(x =>
                x.GetAvailableDriversWithinDistance(
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<int>(),
                    It.IsAny<DriverStatuses>()
                )
            )
            .ReturnsAsync(drivers);
        _mockAddressRepository
            .Setup(x => x.GetAddressById(It.Is<int>(id => id == foodPlace.AddressId)))
            .ReturnsAsync(addresses[0]);
        _mockAddressRepository
            .Setup(x => x.GetAddressById(It.Is<int>(id => id == order.DeliveryAddressId)))
            .ReturnsAsync(addresses[1]);

        // Act
        await _service.InitiateDeliveryAssignment(order);

        // Assert
        _mockClientProxy.Verify(
            x =>
                x.SendCoreAsync(
                    "ReceiveDeliveryOffer",
                    It.Is<object[]>(args =>
                        args.Length == 2 && args[0] is DeliveryOfferDTO && (int)args[1] == order.Id
                    ),
                    default
                ),
            Times.Exactly(drivers.Count)
        );

        foreach (var driver in drivers)
        {
            _mockDriverRepository.Verify(
                x =>
                    x.UpdateDriverStatus(
                        It.Is<AvailableDriver>(d =>
                            d.Id == driver.Id && d.Status == DriverStatuses.offered
                        )
                    ),
                Times.Once
            );
            job.PendingOffers.ContainsKey(driver.Id).Should().BeTrue();
            driver.Status.Should().Be(DriverStatuses.offered);
        }
    }

    [Fact]
    public async Task InitiateDeliveryAssignment_WithNoAvailableDrivers_ShouldScheduleRetry()
    {
        // Arrange
        var order = CreateTestOrder();
        var foodPlace = CreateTestFoodPlace();
        var job = CreateTestDeliveryAssignmentJob(order.Id);
        var emptyDriverList = new List<AvailableDriver>();

        _mockDeliveriesAssignments.Setup(x => x.GetOrCreateAssignmentJob(order.Id)).Returns(job);
        _mockFoodPlaceRepository
            .Setup(x => x.GetFoodPlaceById(order.FoodPlaceId))
            .ReturnsAsync(foodPlace);
        _mockDriverRepository
            .Setup(x =>
                x.GetAvailableDriversWithinDistance(
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<int>(),
                    It.IsAny<DriverStatuses>()
                )
            )
            .ReturnsAsync(emptyDriverList);

        // Act
        await _service.InitiateDeliveryAssignment(order);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync("ReceiveDeliveryOffer", It.IsAny<object[]>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task AcceptDeliveryOffer_WithValidDriverAndOrder_ShouldAssignDriver()
    {
        // Arrange
        var driverId = 1;
        var orderId = 100;
        var job = CreateTestDeliveryAssignmentJob(orderId);
        var driver = CreateTestAvailableDriver(driverId);
        var order = CreateTestOrder(orderId);

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);
        _mockDriverRepository.Setup(x => x.GetDriverById(driverId)).ReturnsAsync(driver);
        _mockOrderRepository.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);

        // Act
        await _service.AcceptDeliveryOffer(driverId, orderId);

        // Assert
        job.AssignedDriverId.Should().Be(driverId);
        driver.Status.Should().Be(DriverStatuses.delivering);
        order.Delivery!.DriverId.Should().Be(driverId);
        order.Delivery.Status.Should().Be(DeliveryStatuses.pickup);

        _mockDriverRepository.Verify(x => x.UpdateDriverStatus(driver), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateDelivery(orderId, order.Delivery), Times.Once);
        _mockClientProxy.Verify(
            x =>
                x.SendCoreAsync(
                    "AssignDelivery",
                    It.Is<object[]>(args => args.Length == 1 && (int)args[0] == orderId),
                    default
                ),
            Times.Once
        );
        _mockDeliveriesAssignments.Verify(x => x.RemoveAssignmentJob(orderId), Times.Once);
    }

    [Fact]
    public async Task AcceptDeliveryOffer_WhenDriverAlreadyAssigned_ShouldReturnEarly()
    {
        // Arrange
        var driverId = 1;
        var orderId = 100;
        var job = CreateTestDeliveryAssignmentJob(orderId);
        job.AssignedDriverId = 999;

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);

        // Act
        await _service.AcceptDeliveryOffer(driverId, orderId);

        // Assert
        job.AssignedDriverId.Should().Be(999);
        _mockDriverRepository.Verify(x => x.GetDriverById(It.IsAny<int>()), Times.Never);
        _mockOrderRepository.Verify(
            x => x.UpdateDelivery(It.IsAny<int>(), It.IsAny<Delivery>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AcceptDeliveryOffer_WithInvalidDriver_ShouldThrowException()
    {
        // Arrange
        var driverId = 1;
        var orderId = 100;
        var job = CreateTestDeliveryAssignmentJob(orderId);

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);
        _mockDriverRepository
            .Setup(x => x.GetDriverById(driverId))
            .ReturnsAsync((AvailableDriver?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AcceptDeliveryOffer(driverId, orderId)
        );

        Assert.Contains($"Driver with ID {driverId} not found", exception.Message);
    }

    [Fact]
    public void RejectDeliveryOffer_WithValidDriverAndOrder_ShouldCancelOffer()
    {
        // Arrange
        var driverId = 1;
        var orderId = 100;
        var job = CreateTestDeliveryAssignmentJob(orderId);
        var cts = new CancellationTokenSource();
        job.PendingOffers[driverId] = cts;

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);

        // Act
        _service.RejectDeliveryOffer(driverId, orderId);

        // Assert
        job.PendingOffers.ContainsKey(driverId).Should().BeFalse();
        cts.Token.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void RejectDeliveryOffer_WithNonExistentOffer_ShouldNotThrow()
    {
        // Arrange
        var driverId = 1;
        var orderId = 100;
        var job = CreateTestDeliveryAssignmentJob(orderId);

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);

        // Act
        var act = () => _service.RejectDeliveryOffer(driverId, orderId);

        // Assert
        act.Should().NotThrow();
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

    private List<AvailableDriver> CreateTestAvailableDrivers(int count)
    {
        var drivers = new List<AvailableDriver>();
        for (int i = 1; i <= count; i++)
        {
            drivers.Add(CreateTestAvailableDriver(i));
        }
        return drivers;
    }

    private AvailableDriver CreateTestAvailableDriver(int id)
    {
        return new AvailableDriver
        {
            Id = id,
            Status = DriverStatuses.online,
            Distance = 500.0,
        };
    }

    private DeliveryAssignmentJob CreateTestDeliveryAssignmentJob(int orderId)
    {
        return new DeliveryAssignmentJob
        {
            OrderId = orderId,
            CurrentAttempt = 0,
            AssignedDriverId = 0,
            PendingOffers = new ConcurrentDictionary<int, CancellationTokenSource>(),
        };
    }

    private List<Address> CreateTestAddresses()
    {
        return new List<Address>
        {
            new Address
            {
                NumberAndStreet = "123 Food St",
                City = "Test City",
                Postcode = "W8 8BB",
            },
            new Address
            {
                NumberAndStreet = "456 Customer Avenue",
                City = "Test City",
                Postcode = "W18 18BB",
            },
        };
    }
}
