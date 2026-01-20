using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Tests.Api;

public class OrdersControllerTests : IntegrationTestBase
{
    private readonly DatabaseHelper _databaseHelper;

    public OrdersControllerTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
        _databaseHelper = new DatabaseHelper(fixture.PostgresConnectionString);
    }

    private object ConstructStripeSuccessPayload(int orderId, string paymentIntentId, int amount)
    {
        var fixturePath = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "paymentIntentSucceededEventFixture.json"
        );
        string jsonString = File.ReadAllText(fixturePath);
        var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString)!;

        var dataElement = (JsonElement)jsonDict["data"];
        var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            dataElement.GetRawText()
        )!;

        var objectElement = (JsonElement)dataDict["object"];
        var objectDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
            objectElement.GetRawText()
        )!;
        objectDict["id"] = paymentIntentId;
        objectDict["amount"] = amount;
        objectDict["amount_received"] = amount;
        objectDict["metadata"] = new Dictionary<string, string>
        {
            { "order_id", orderId.ToString() },
        };

        dataDict["object"] = objectDict;
        jsonDict["data"] = dataDict;

        return jsonDict;
    }

    [Fact]
    public async Task CreateOrder_ValidCartItems_CreatesOrderWithPendingPaymentStatus()
    {
        // Arrange
        await TestDataBuilder.AddTestCartItem(
            Consts.CustomerPhoneNumber,
            itemId: 1,
            quantity: Consts.Quantities.Default
        );

        var customerToken = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var authenticatedClient = Client.WithAuth(customerToken);

        // Act
        var reqBody = new CreateOrderCommand { DeliveryAddress = TestDataBuilder.addressReq };
        var response = await authenticatedClient.PostAsJsonAsync(Consts.Urls.orders, reqBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createOrderDto = await response.Content.ReadFromJsonAsync<CreateOrderDTO>();
        createOrderDto.Should().NotBeNull();
        createOrderDto!.OrderId.Should().BeGreaterThan(0);
        createOrderDto.ClientSecret.Should().NotBeNullOrEmpty();

        int orderId = createOrderDto.OrderId;

        var customerId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.CustomerPhoneNumber);
        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );

        var order = await _databaseHelper.GetOrderById(orderId);
        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatuses.pendingPayment);
        order.CustomerId.Should().Be(customerId);
        order.FoodPlaceId.Should().Be(foodPlaceId);

        var orderItems = await _databaseHelper.GetOrderItemsByOrderId(orderId);
        orderItems.Should().HaveCount(1);
        var orderItem = orderItems.First();
        orderItem.Quantity.Should().Be(Consts.Quantities.Default);
        orderItem.ItemId.Should().Be(1);

        // Verify payment
        var payment = await _databaseHelper.GetPaymentByOrderId(orderId);
        payment.Should().NotBeNull();
        payment!.Status.Should().Be(PaymentStatuses.NotConfirmed);
        payment.StripePaymentIntentId.Should().NotBeNullOrEmpty();

        // Verify cart is cleared
        var cartItemCount = await _databaseHelper.GetCartItemCount(customerId);
        cartItemCount.Should().Be(0);
    }

    [Fact]
    public async Task CancelOrder_CompletedPayment_RefundsPayment()
    {
        // Arrange
        var customerId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.CustomerPhoneNumber);
        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
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

        await _databaseHelper.CreateDelivery(orderId, DeliveryStatuses.assigningDriver);

        // Use a temp placeholder id for stripe paymentIntent id
        await _databaseHelper.CreatePayment(
            orderId,
            2820,
            PaymentStatuses.Completed,
            "pi_placeholder_id"
        );

        // we need the real repo method because of the mapping to the domain entity
        using var scope = Factory.Services.CreateScope();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var orderBefore = await orderRepo.GetOrderById(orderId);
        var stripeService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var createdIntent = stripeService.CreatePaymentIntent(
            orderBefore!,
            new Address
            {
                NumberAndStreet = TestDataBuilder.addressReq.NumberAndStreet,
                City = TestDataBuilder.addressReq.City,
                Postcode = TestDataBuilder.addressReq.Postcode,
            }
        );
        await _databaseHelper.UpdatePaymentIntentId(orderId, createdIntent.Id);
        var paymentIntentService = new Stripe.PaymentIntentService();
        var confirmedIntent = await paymentIntentService.ConfirmAsync(
            createdIntent.Id,
            new Stripe.PaymentIntentConfirmOptions { PaymentMethod = "pm_card_visa" }
        );
        stripeService.CapturePaymentIntent(createdIntent.Id);

        var foodPlaceToken = await AuthHelper.LogInUser(
            Consts.FoodPlacePhoneNumber,
            Consts.TestPassword,
            Client
        );
        var cancelRequest = new { Reason = "test refund" };

        // Act
        var response = await Client
            .WithAuth(foodPlaceToken)
            .PatchAsJsonAsync($"{Consts.Urls.ordersCancel}{orderId}", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderAfter = await _databaseHelper.GetOrderById(orderId);
        orderAfter.Should().NotBeNull();
        orderAfter!.Status.Should().Be(OrderStatuses.cancelled);

        var paymentAfter = await _databaseHelper.GetPaymentByOrderId(orderId);
        paymentAfter.Should().NotBeNull();
        paymentAfter!.Status.Should().Be(PaymentStatuses.Refunded);

        var refund = await stripeService.GetRefundByPaymentIntentId(createdIntent.Id);
        ((string)refund.Status).Should().Be("succeeded");
    }

    [Fact]
    public async Task CompleteOrderFlow_FromCartToDriverAcceptance_SuccessfullyCompletesAllStages()
    {
        // Step 1: Create order from cart
        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
        await TestDataBuilder.AddTestCartItem(
            Consts.CustomerPhoneNumber,
            foodPlaceItem.Id,
            Consts.Quantities.Default
        );

        var customerToken = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var reqBody = new CreateOrderCommand { DeliveryAddress = TestDataBuilder.addressReq };
        var response = await Client
            .WithAuth(customerToken)
            .PostAsJsonAsync(Consts.Urls.orders, reqBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createOrderDto = await response.Content.ReadFromJsonAsync<CreateOrderDTO>();
        createOrderDto.Should().NotBeNull();
        var orderId = createOrderDto!.OrderId;
        orderId.Should().BeGreaterThan(0);
        createOrderDto.ClientSecret.Should().NotBeNullOrEmpty();

        using var scope = Factory.Services.CreateScope();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var order = await orderRepo.GetOrderById(orderId);
        order!.Status.Should().Be(OrderStatuses.pendingPayment);

        var paymentIntentService = new Stripe.PaymentIntentService();
        var confirmedIntent = await paymentIntentService.ConfirmAsync(
            order.Payment!.StripePaymentIntentId,
            new Stripe.PaymentIntentConfirmOptions { PaymentMethod = "pm_card_visa" }
        );

        // Step 2: Set up food place SignalR connection to receive confirmation request
        var foodPlaceToken = await AuthHelper.LogInUser(
            Consts.FoodPlacePhoneNumber,
            Consts.TestPassword,
            Client
        );
        await using var foodPlaceConnection = await SignalRHelper.CreateFoodPlaceConnection(
            BaseUrl,
            foodPlaceToken
        );

        // Listen for confirmation request and confirm via HTTP when received
        var confirmedOrderId = -1;
        foodPlaceConnection.On<OrderConfirmationDTO>(
            "ReceiveOrderConfirmation",
            async (confirmationDto) =>
            {
                int receivedOrderId = confirmationDto.OrderId;
                confirmedOrderId = receivedOrderId;

                var confirmRequest = new { Confirmed = true };
                await Client
                    .WithAuth(foodPlaceToken)
                    .PatchAsJsonAsync(
                        $"{Consts.Urls.ordersConfirm}{receivedOrderId}",
                        confirmRequest
                    );
            }
        );

        // Step 3: Set up driver SignalR connection to receive and accept delivery offer
        var driverToken = await AuthHelper.LogInUser(
            Consts.DriverPhoneNumber,
            Consts.TestPassword,
            Client
        );
        await using var driverConnection = await SignalRHelper.CreateDriverConnection(
            BaseUrl,
            driverToken
        );

        // Listen for delivery offer and accept it
        var deliveryOrderId = -1;
        driverConnection.On<DeliveryOfferDTO, int>(
            "ReceiveDeliveryOffer",
            async (_, offerOrderId) =>
            {
                deliveryOrderId = offerOrderId;
                await driverConnection.InvokeAsync("AcceptDeliveryOffer", offerOrderId);
            }
        );
        var driverId = await _databaseHelper.GetUserIdByPhoneNumber(Consts.DriverPhoneNumber);
        await RedisHelper.SeedDriverLocation(
            driverId,
            Consts.LondonCentralLat,
            Consts.LondonCentralLong
        );

        // Step 4: Send Stripe webhook
        var payment = await _databaseHelper.GetPaymentByOrderId(orderId);
        payment.Should().NotBeNull();
        var paymentIntentId = payment!.StripePaymentIntentId!;
        var webhookPayload = ConstructStripeSuccessPayload(orderId, paymentIntentId, 2890);
        var webhookResponse = await Client.PostAsJsonAsync(
            Consts.Urls.stripeWebhook,
            webhookPayload
        );

        webhookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        confirmedOrderId.Should().Be(orderId);

        deliveryOrderId.Should().Be(orderId);

        // Verify order is in preparing state with delivery assigned
        order = await _databaseHelper.GetOrderById(orderId);
        order!.Status.Should().Be(OrderStatuses.preparing);

        payment = await _databaseHelper.GetPaymentByOrderId(orderId);
        payment.Should().NotBeNull();
        payment!.Status.Should().Be(PaymentStatuses.Completed);

        var delivery = await _databaseHelper.GetDeliveryByOrderId(orderId);
        delivery.Should().NotBeNull();
        delivery!.DriverId.Should().NotBe(0);
        delivery.Status.Should().Be(DeliveryStatuses.pickup);
        delivery.ConfirmationCode.Should().NotBeNullOrEmpty();

        // Step 5: Food Place Updates Order Status
        var updateRequest = new { Status = OrderStatuses.readyForPickup };
        var updateResponse = await Client
            .WithAuth(foodPlaceToken)
            .PatchAsJsonAsync($"{Consts.Urls.ordersStatus}{orderId}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        order = await _databaseHelper.GetOrderById(orderId);
        order!.Status.Should().Be(OrderStatuses.readyForPickup);

        // Update to delivering
        updateRequest = new { Status = OrderStatuses.delivering };
        await Client
            .WithAuth(foodPlaceToken)
            .PatchAsJsonAsync($"{Consts.Urls.ordersStatus}{orderId}", updateRequest);

        order = await _databaseHelper.GetOrderById(orderId);
        order!.Status.Should().Be(OrderStatuses.delivering);

        // Step 6: Complete Order
        var completeRequest = new { Status = OrderStatuses.completed };
        await Client
            .WithAuth(foodPlaceToken)
            .PatchAsJsonAsync($"{Consts.Urls.ordersStatus}{orderId}", completeRequest);

        order = await _databaseHelper.GetOrderById(orderId);
        order!.Status.Should().Be(OrderStatuses.completed);
    }
}
