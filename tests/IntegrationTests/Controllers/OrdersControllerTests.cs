using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json;

public class OrdersControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _orderItemsRepo;
    private readonly QuotesRepository _quotesRepo;
    private readonly QuoteTokenService _tokenService;
    private readonly TestDataSeeder _seeder;

    public OrdersControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _tokenService = _factory.GetServiceFromContainer<QuoteTokenService>();
        _ordersRepo = _factory.GetServiceFromContainer<OrdersRepository>();
        _orderItemsRepo = _factory.GetServiceFromContainer<OrdersItemsRepository>();
        _quotesRepo = _factory.GetServiceFromContainer<QuotesRepository>();
        _seeder = _factory.GetServiceFromContainer<TestDataSeeder>();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldCreateOrderAndItems()
    {
        // Arrange
        var quoteId = await _seeder.SeedQuoteAndQuoteItems();
        var quoteItems = TestData.Orders.itemRequests;
        var payload = new QuoteTokenPayload
        {
            CustomerId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Items = quoteItems,
            TotalPrice = 2070
        };

        var request = new CreateOrderRequest
        {
            QuoteId = quoteId,
            QuoteToken = _tokenService.GenerateQuoteToken(payload),
            QuoteTokenPayload = payload
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Orders, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderResponse>();
        int orderId = result!.OrderId;
        orderId.Should().BeGreaterThan(0);

        // Verify database entries using repositories
        var order = await _ordersRepo.GetOrderById(orderId);
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(payload.CustomerId);

        var orderItems = await _orderItemsRepo.GetOrderItemsByOrderId(orderId);
        orderItems.Should().HaveSameCount(quoteItems);

        foreach (var orderItem in orderItems)
        {
            var matchingQuoteItem = quoteItems.First(qi => qi.ItemId == orderItem.ItemId);
            orderItem.Quantity.Should().Be(matchingQuoteItem.Quantity);
        }

        // Verify quote is marked as used
        var updatedQuote = await _quotesRepo.GetQuoteById(quoteId);
        updatedQuote!.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrder_WithInvalidQuoteToken_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new QuoteTokenPayload
        {
            CustomerId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Items = TestData.Orders.itemRequests,
            TotalPrice = 2070
        };

        var request = new CreateOrderRequest
        {
            QuoteId = 1,
            QuoteToken = "invalid.token",
            QuoteTokenPayload = payload
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Orders, request);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentQuote_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = new QuoteTokenPayload
        {
            CustomerId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Items = TestData.Orders.itemRequests,
            TotalPrice = 2070
        };

        var request = new CreateOrderRequest
        {
            QuoteId = 999999999,
            QuoteToken = _tokenService.GenerateQuoteToken(payload),
            QuoteTokenPayload = payload
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Orders, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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