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
    private readonly Mock<IJourneyCalculationService> _mockJourneyCalculationService;
    private readonly Mock<IHubContext<DriverHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IDeliveriesAssignments> _mockDeliveriesAssignments;
    private readonly Mock<IDriverPaymentService> _mockDriverPaymentService;
    private readonly Mock<ILogger<DeliveryAssignmentService>> _mockLogger;
    private readonly DeliveryAssignmentService _service;

    public DeliveryAssignmentServiceTests()
    {
        _mockDriverRepository = new Mock<IDriverRepository>();
        _mockFoodPlaceRepository = new Mock<IFoodPlaceRepository>();
        _mockAddressRepository = new Mock<IAddressRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockJourneyCalculationService = new Mock<IJourneyCalculationService>();
        _mockHubContext = new Mock<IHubContext<DriverHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockDeliveriesAssignments = new Mock<IDeliveriesAssignments>();
        _mockDriverPaymentService = new Mock<IDriverPaymentService>();
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
            _mockDriverPaymentService.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(10),
            TimeSpan.FromMilliseconds(5)
        );
    }

    [Fact]
    public async Task InitiateDeliveryAssignment_WithAvailableDrivers_ShouldSendOffersToAllDrivers()
    {
        // Arrange
        var order = OrderTestsHelper.CreateTestOrder();
        var foodPlace = OrderTestsHelper.CreateTestFoodPlace();
        var drivers = OrderTestsHelper.CreateTestAvailableDrivers(2);
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(order.Id);
        var addresses = OrderTestsHelper.CreateTestAddresses();

        _mockDeliveriesAssignments.Setup(x => x.CreateAssignmentJob(order.Id)).Returns(job);
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
        _mockJourneyCalculationService
            .Setup(x => x.GeocodeAddressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderTestsHelper.CreateTestLocation());
        _mockJourneyCalculationService
            .Setup(x =>
                x.CalculateRouteAsync(
                    It.IsAny<Location>(),
                    It.IsAny<Location>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(OrderTestsHelper.CreateTestMapboxRoute());
        _mockJourneyCalculationService
            .Setup(x => x.CreateCombinedRoute(It.IsAny<MapboxRoute>(), It.IsAny<MapboxRoute>()))
            .Returns(OrderTestsHelper.CreateTestMapboxRoute());
        _mockDriverPaymentService
            .Setup(x => x.CalculatePayment(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(500);

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
            Times.AtLeast(drivers.Count) // number of retries is private
        );

        foreach (var driver in drivers)
        {
            _mockDriverRepository.Verify(
                x => x.UpdateDriverStatus(It.Is<AvailableDriver>(d => d.Id == driver.Id)),
                Times.AtLeastOnce
            );
            driver.Status.Should().Be(DriverStatuses.online);
        }
    }

    [Fact]
    public async Task InitiateDeliveryAssignment_WithNoAvailableDrivers_ShouldScheduleRetry()
    {
        // Arrange
        var order = OrderTestsHelper.CreateTestOrder();
        var foodPlace = OrderTestsHelper.CreateTestFoodPlace();
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(order.Id);
        var emptyDriverList = new List<AvailableDriver>();

        _mockDeliveriesAssignments.Setup(x => x.CreateAssignmentJob(order.Id)).Returns(job);
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
        var addresses = OrderTestsHelper.CreateTestAddresses();
        _mockAddressRepository
            .Setup(x => x.GetAddressById(It.Is<int>(id => id == order.DeliveryAddressId)))
            .ReturnsAsync(addresses[1]);
        _mockJourneyCalculationService
            .Setup(x => x.GeocodeAddressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderTestsHelper.CreateTestLocation());

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
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(orderId);
        var driver = OrderTestsHelper.CreateTestAvailableDriver(driverId);
        var order = OrderTestsHelper.CreateTestOrder(orderId);
        var testRoute = OrderTestsHelper.CreateTestMapboxRoute();
        // Add route to job to simulate it being stored during offer creation
        job.DriversRoutes[driverId] = testRoute;

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);
        _mockDriverRepository.Setup(x => x.GetDriverById(driverId)).ReturnsAsync(driver);
        _mockOrderRepository.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _mockDriverPaymentService
            .Setup(x => x.CalculatePayment(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(500);

        // Act
        await _service.AcceptDeliveryOffer(driverId, orderId);

        // Assert
        job.AssignedDriverId.Should().Be(driverId);
        driver.Status.Should().Be(DriverStatuses.delivering);
        order.Delivery!.DriverId.Should().Be(driverId);
        order.Delivery.Status.Should().Be(DeliveryStatuses.pickup);
        order.Delivery.Route.Should().Be(testRoute);
        order.Delivery.PaymentAmount.Should().Be(500);

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
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(orderId);
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
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(orderId);

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
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(orderId);
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
        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(orderId);

        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);

        // Act
        var act = () => _service.RejectDeliveryOffer(driverId, orderId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CancelOngoingAssignment_ShouldCancelAllOffers_WhenJobExists()
    {
        // Arrange
        var orderId = 123;
        var driverId = 1;

        var job = OrderTestsHelper.CreateTestDeliveryAssignmentJob(orderId);
        var cts = new CancellationTokenSource();
        job.PendingOffers[driverId] = cts;
        _mockDeliveriesAssignments.Setup(x => x.GetAssignmentJob(orderId)).Returns(job);

        // Act
        _service.CancelOngoingAssignment(orderId);

        // Assert
        job.PendingOffers.Should().BeEmpty();
        cts.Token.IsCancellationRequested.Should().BeTrue();
        _mockDeliveriesAssignments.Verify(x => x.RemoveAssignmentJob(orderId), Times.Once);
    }

    [Fact]
    public void CancelOngoingAssignment_ShouldDoNothing_WhenJobDoesNotExist()
    {
        // Arrange
        var orderId = 123;
        _mockDeliveriesAssignments
            .Setup(x => x.GetAssignmentJob(orderId))
            .Returns((DeliveryAssignmentJob?)null);

        // Act
        _service.CancelOngoingAssignment(orderId);

        // Assert
        _mockDriverRepository.Verify(x => x.GetDriverById(It.IsAny<int>()), Times.Never);
        _mockDriverRepository.Verify(
            x => x.UpdateDriverStatus(It.IsAny<AvailableDriver>()),
            Times.Never
        );
        _mockDeliveriesAssignments.Verify(x => x.RemoveAssignmentJob(It.IsAny<int>()), Times.Never);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never
        );
    }
}
