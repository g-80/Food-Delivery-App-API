using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

[Collection("Controllers collection")]
public class OrdersControllerTests
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _orderItemsRepo;
    private readonly CartsRepository _cartsRepo;
    private readonly TestDataSeeder _seeder;

    public OrdersControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _ordersRepo = _factory.GetServiceFromContainer<OrdersRepository>();
        _orderItemsRepo = _factory.GetServiceFromContainer<OrdersItemsRepository>();
        _cartsRepo = _factory.GetServiceFromContainer<CartsRepository>();
        _seeder = _factory.GetServiceFromContainer<TestDataSeeder>();
        _factory.SetCustomerAccessToken();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldCreateOrderAndItems()
    {
        // Act
        var response = await _factory.Client.PostAsync(HttpHelper.Urls.Orders, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderResponse>();
        int orderId = result!.OrderId;
        orderId.Should().BeGreaterThan(0);

        // Verify database entries using repositories
        var order = await _ordersRepo.GetOrderById(orderId);
        order.Should().NotBeNull();

        var orderItems = await _orderItemsRepo.GetOrderItemsByOrderId(orderId);
        orderItems.Should().HaveSameCount(TestData.Carts.itemRequests);

        foreach (var orderItem in orderItems)
        {
            var matchingCartItem = TestData.Carts.CreateCartItemDTOs().First(cartItem => cartItem.RequestedItem.ItemId == orderItem.ItemId);
            orderItem.Quantity.Should().Be(matchingCartItem.RequestedItem.Quantity);
        }

        // Verify quote is marked as used
        var updatedQuote = await _cartsRepo.GetCartById(TestData.Carts.assignedCartId);
        updatedQuote!.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task CancelOrder_WithValidId_ShouldCancelOrder()
    {
        // Arrange
        var orderId = await _seeder.SeedOrderAndOrderItems();

        // Act
        var response = await _factory.Client.PatchAsync(HttpHelper.Urls.CancelOrder + orderId, null);

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
        var orderId = await _seeder.SeedOrderAndOrderItems();

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Orders + orderId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        returnedOrder.Should().NotBeNull();
        returnedOrder!.OrderId.Should().Be(orderId);
    }
}