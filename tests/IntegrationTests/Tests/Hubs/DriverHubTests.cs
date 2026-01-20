using FluentAssertions;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Tests.Hubs;

public class DriverHubTests : IntegrationTestBase
{
    private readonly DatabaseHelper _databaseHelper;

    public DriverHubTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
        _databaseHelper = new DatabaseHelper(fixture.PostgresConnectionString);
    }

    private async Task<bool> TriggerDeliveryAssignment(int orderId)
    {
        var order = await _databaseHelper.GetOrderById(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found");
        }

        using var scope = Factory.Services.CreateScope();
        var deliveryAssignmentService =
            scope.ServiceProvider.GetRequiredService<IDeliveryAssignmentService>();

        return await deliveryAssignmentService.InitiateDeliveryAssignment(order);
    }

    [Fact]
    public async Task OnConnect_MarksDriverOnlineAndSendsStatusUpdate()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.DriverPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);

        // Act
        await using var connection = await SignalRHelper.CreateDriverConnection(BaseUrl, token);
        // Assert
        var redisStatus = await RedisHelper.GetDriverStatus(driverId);
        redisStatus.Should().Be("1");
    }

    [Fact]
    public async Task OnDisconnect_MarksDriverOffline()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.DriverPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);

        await using var connection = await SignalRHelper.CreateDriverConnection(BaseUrl, token);

        var initialStatus = await RedisHelper.GetDriverStatus(driverId);
        initialStatus.Should().Be("1");

        // Act
        await connection.DisposeAsync();

        // Assert
        var statusCleared = await AsyncWaitHelper.WaitForConditionAsync(
            async () => string.IsNullOrEmpty(await RedisHelper.GetDriverStatus(driverId)),
            timeout: TimeSpan.FromSeconds(2)
        );
        statusCleared.Should().BeTrue("driver status should be cleared after disconnect");
    }

    [Fact]
    public async Task UpdateLocation_WithoutDeliveryId_UpdatesRedisOnly()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.DriverPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);

        await using var connection = await SignalRHelper.CreateDriverConnection(BaseUrl, token);

        // Act
        await connection.InvokeAsync("UpdateLocation", 51.5074, -0.1278, null, null, null, null);

        // Assert
        var location = await RedisHelper.GetDriverLocation(driverId);
        location.Should().NotBeNull();
        location.Latitude.Should().BeApproximately(51.5074, 0.0001);
        location.Longitude.Should().BeApproximately(-0.1278, 0.0001);

        var timestamp = await RedisHelper.GetDriverLocationTimestamp(driverId);
        timestamp.Should().NotBeNull();
        timestamp.Should().BeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds());

        var historyCount = await _databaseHelper.GetLocationHistoryCount(driverId);
        historyCount.Should().Be(0);
    }

    [Fact]
    public async Task UpdateLocation_WithDeliveryId_UpdatesRedisAndDatabase()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.DriverPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);
        var customerId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.CustomerPhoneNumber);
        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );

        var orderId = await _databaseHelper.CreateOrder(
            customerId,
            foodPlaceId,
            OrderStatuses.preparing
        );
        var deliveryId = await _databaseHelper.CreateDelivery(
            orderId,
            DeliveryStatuses.pickup,
            driverId
        );

        await using var connection = await SignalRHelper.CreateDriverConnection(BaseUrl, token);

        // Act
        await connection.InvokeAsync(
            "UpdateLocation",
            51.5074,
            -0.1278,
            10.0,
            5.5,
            180.0,
            deliveryId
        );

        // Assert
        var location = await RedisHelper.GetDriverLocation(driverId);
        location.Should().NotBeNull();
        location.Latitude.Should().BeApproximately(51.5074, 0.0001);
        location.Longitude.Should().BeApproximately(-0.1278, 0.0001);

        var history = await _databaseHelper.GetLocationHistory(driverId, deliveryId);
        history.Should().HaveCount(1);

        var record = history.First();
        record.DeliveryId.Should().Be(deliveryId);
        record.Latitude.Should().BeApproximately(51.5074, 0.0001);
        record.Longitude.Should().BeApproximately(-0.1278, 0.0001);
        record.Speed.Should().Be(5.5);
        record.Heading.Should().Be(180.0);
    }

    [Fact]
    public async Task AcceptDeliveryOffer_ValidOffer_AssignsDriver()
    {
        // Arrange
        var driverToken = await AuthHelper.LogInUser(
            Consts.DriverPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);
        var customerId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.CustomerPhoneNumber);
        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );

        await RedisHelper.SeedDriverLocation(
            driverId,
            Consts.LondonCentralLat,
            Consts.LondonCentralLong
        );

        var orderId = await _databaseHelper.CreateOrder(
            customerId,
            foodPlaceId,
            OrderStatuses.preparing
        );
        var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
        await _databaseHelper.CreateOrderItemForOrder(
            orderId,
            foodPlaceItem.Id,
            Consts.Quantities.Default,
            Consts.Prices.DefaultItemPrice
        );
        await _databaseHelper.CreatePayment(
            orderId,
            1120,
            PaymentStatuses.Completed,
            "pi_testpaymentintent_id"
        );
        var deliveryId = await _databaseHelper.CreateDelivery(
            orderId,
            DeliveryStatuses.assigningDriver
        );

        await using var driverConnection = await SignalRHelper.CreateDriverConnection(
            BaseUrl,
            driverToken
        );

        var receivedOrderId = -1;
        driverConnection.On<dynamic, int>(
            "ReceiveDeliveryOffer",
            (_, oid) => receivedOrderId = oid
        );

        var assignmentTask = Task.Run(async () => await TriggerDeliveryAssignment(orderId));

        // Act
        // Wait for the delivery offer to arrive via websocket
        var offerReceived = await AsyncWaitHelper.WaitForConditionAsync(
            () => Task.FromResult(receivedOrderId != -1),
            timeout: TimeSpan.FromMilliseconds(250)
        );
        offerReceived.Should().BeTrue("delivery offer should be received");

        await driverConnection.InvokeAsync("AcceptDeliveryOffer", orderId);

        // Wait for assignment task to complete
        var assignmentSucceeded = await assignmentTask.WaitAsync(TimeSpan.FromMilliseconds(250));

        // Assert
        receivedOrderId.Should().Be(orderId);
        assignmentSucceeded.Should().BeTrue();

        var delivery = await _databaseHelper.GetDeliveryByOrderId(orderId);
        delivery.Should().NotBeNull();
        delivery.DriverId.Should().Be(driverId);
        delivery.Status.Should().Be(DeliveryStatuses.pickup);
        delivery.RouteJson.Should().NotBeNullOrEmpty();
        delivery.PaymentAmount.Should().BeGreaterThan(0);

        var driverStatus = await RedisHelper.GetDriverStatus(driverId);
        driverStatus.Should().Be(((int)DriverStatuses.delivering).ToString());
    }

    // [Fact]
    // public async Task RejectDeliveryOffer_ValidOffer_ResetsDriverStatus()
    // {
    //     // Arrange
    //     var driverToken = await AuthHelper.LogInUser(
    //         Consts.DriverPhoneNumber,
    //         Consts.TestPassword,
    //         Client
    //     );
    //     var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);
    //     var customerId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.CustomerPhoneNumber);
    //     var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
    //         Consts.FoodPlacePhoneNumber
    //     );

    //     await RedisHelper.SeedDriverLocation(
    //         driverId,
    //         Consts.LondonCentralLat,
    //         Consts.LondonCentralLong
    //     );

    //     var orderId = await _databaseHelper.CreateOrder(
    //         customerId,
    //         foodPlaceId,
    //         OrderStatuses.preparing
    //     );
    //     var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
    //     await _databaseHelper.CreateOrderItemForOrder(
    //         orderId,
    //         foodPlaceItem.Id,
    //         Consts.Quantities.Default,
    //         Consts.Prices.DefaultItemPrice
    //     );
    //     await _databaseHelper.CreatePayment(
    //         orderId,
    //         1120,
    //         PaymentStatuses.Completed,
    //         "pi_testpaymentintent_id"
    //     );
    //     var deliveryId = await _databaseHelper.CreateDelivery(
    //         orderId,
    //         DeliveryStatuses.assigningDriver
    //     );

    //     await using var driverConnection = await SignalRHelper.CreateDriverConnection(
    //         BaseUrl,
    //         driverToken
    //     );

    //     var receivedOrderId = -1;
    //     driverConnection.On<dynamic, int>(
    //         "ReceiveDeliveryOffer",
    //         (_, oid) => receivedOrderId = oid
    //     );

    //     _ = Task.Run(
    //         async () =>
    //             await TestDataBuilder.TriggerDeliveryAssignment(
    //                 Fixture.PostgresConnectionString,
    //                 orderId,
    //                 Factory
    //             )
    //     );

    //     // Act
    //     // Wait for the delivery offer to arrive via websocket
    //     var offerReceived = await AsyncWaitHelper.WaitForConditionAsync(
    //         () => Task.FromResult(receivedOrderId != -1),
    //         timeout: TimeSpan.FromMilliseconds(300),
    //         pollingInterval: TimeSpan.FromMilliseconds(25)
    //     );
    //     offerReceived.Should().BeTrue("delivery offer should be received");

    //     await driverConnection.InvokeAsync("RejectDeliveryOffer", orderId);

    //     // Assert
    //     var driverStatus = await RedisHelper.GetDriverStatus(driverId);
    //     driverStatus.Should().Be(((int)DriverStatuses.online).ToString());

    //     var delivery = await _databaseHelper.GetDeliveryByOrderId(orderId);
    //     delivery.Should().NotBeNull();
    //     delivery.DriverId.Should().Be(0);
    // }
}
