using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

[Collection("Controllers collection")]
public class OrdersControllerTests
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly IOrdersRepository _ordersRepo;
    private readonly IOrdersItemsRepository _orderItemsRepo;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly TestDataSeeder _seeder;

    public OrdersControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _ordersRepo = _factory.GetServiceFromContainer<IOrdersRepository>();
        _orderItemsRepo = _factory.GetServiceFromContainer<IOrdersItemsRepository>();
        _cartItemsRepo = _factory.GetServiceFromContainer<ICartItemsRepository>();
        _seeder = _factory.GetServiceFromContainer<TestDataSeeder>();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldCreateOrderAndItems()
    {
        // Arrange
        await _factory.LoginAsACustomerAsync();
        // Act
        var response = await _factory.Client.PostAsync(HttpHelper.Urls.Orders, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderResponse>();
        int orderId = result!.OrderId;
        orderId.Should().BeGreaterThan(0);

        var order = await _ordersRepo.GetOrderById(orderId);
        order.Should().NotBeNull();

        var orderItems = await _orderItemsRepo.GetOrderItemsByOrderId(orderId);
        orderItems.Should().HaveSameCount(TestData.Carts.itemRequests);

        foreach (var orderItem in orderItems)
        {
            var matchingCartItem = TestData
                .Carts.CreateCartItemDTOs()
                .First(cartItem => cartItem.RequestedItem.ItemId == orderItem.ItemId);
            orderItem.Quantity.Should().Be(matchingCartItem.RequestedItem.Quantity);
        }

        var cartItems = await _cartItemsRepo.GetCartItemsByCartId(TestData.Carts.assignedCartId);
        cartItems.Count().Should().Be(0);
    }

    [Fact]
    public async Task CancelOrder_WithValidId_ShouldCancelOrder()
    {
        // Arrange
        await _seeder.SeedOrderAndOrderItems();
        var orderId = TestData.Orders.assignedIds[0];

        // Act
        var response = await _factory.Client.PatchAsync(
            HttpHelper.Urls.CancelOrder + orderId,
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelledOrder = await _ordersRepo.GetOrderById(orderId);
        cancelledOrder.Should().NotBeNull();
        cancelledOrder!.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        await _seeder.SeedOrderAndOrderItems();
        var orderId = TestData.Orders.assignedIds[0];

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Orders + orderId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        returnedOrder.Should().NotBeNull();
        returnedOrder!.OrderId.Should().Be(orderId);
    }
}
