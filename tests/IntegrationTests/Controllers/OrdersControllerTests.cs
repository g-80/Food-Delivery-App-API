using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class OrdersControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly OrdersRepository _ordersRepo;
    private readonly OrderItemsRepository _orderItemsRepo;
    private readonly QuotesRepository _quotesRepo;
    private readonly QuotesItemsRepository _quotesItemsRepo;
    private readonly QuoteTokenService _tokenService;

    public OrdersControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _tokenService = _factory.GetRepoFromServices<QuoteTokenService>();
        _ordersRepo = _factory.GetRepoFromServices<OrdersRepository>();
        _orderItemsRepo = _factory.GetRepoFromServices<OrderItemsRepository>();
        _quotesRepo = _factory.GetRepoFromServices<QuotesRepository>();
        _quotesItemsRepo = _factory.GetRepoFromServices<QuotesItemsRepository>();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldCreateOrderAndItems()
    {
        // Arrange
        var (quoteId, quoteItems) = await CreateTestQuoteAndQuoteItems();
        var payload = new QuoteTokenPayload
        {
            CustomerId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Items = quoteItems,
            TotalPrice = 2070
        };

        var request = new OrderRequest
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
            Items = Fixtures.itemRequests,
            TotalPrice = 2070
        };

        var request = new OrderRequest
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
            Items = Fixtures.itemRequests,
            TotalPrice = 2070
        };

        var request = new OrderRequest
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
        var order = await CreateTestOrderAndOrderItems();

        // Act
        var response = await _factory.Client.PatchAsync(HttpHelper.Urls.CancelOrder + order!.Id, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelledOrder = await _ordersRepo.GetOrderById(order.Id);
        cancelledOrder.Should().NotBeNull();
        cancelledOrder!.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        var order = await CreateTestOrderAndOrderItems();

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Orders + order!.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedOrder = await response.Content.ReadFromJsonAsync<Order>();
        returnedOrder.Should().NotBeNull();
        returnedOrder!.Id.Should().Be(order.Id);
        returnedOrder.CustomerId.Should().Be(order.CustomerId);
    }

    private async Task<(int, List<ItemRequest>)> CreateTestQuoteAndQuoteItems()
    {
        int customerId = 1;
        var quoteItems = Fixtures.itemRequests;
        List<int> prices = new() { Fixtures.itemsFixtures[0].Price * quoteItems[0].Quantity, Fixtures.itemsFixtures[1].Price * quoteItems[1].Quantity };
        int totalPrice = prices.Sum();
        var quoteId = await _quotesRepo.CreateQuote(customerId, totalPrice, DateTime.UtcNow.AddMinutes(5));
        await Task.WhenAll(quoteItems.Select((item, i) =>
            _quotesItemsRepo.CreateQuoteItem(
                item,
                quoteId,
                prices[i]
            )
        ));

        return (quoteId, quoteItems);
    }

    private async Task<Order?> CreateTestOrderAndOrderItems()
    {
        var orderData = new CustomerItemsRequest
        {
            CustomerId = 1,
            Items = Fixtures.itemRequests
        };
        List<int> prices = new() { Fixtures.itemsFixtures[0].Price * orderData.Items[0].Quantity, Fixtures.itemsFixtures[1].Price * orderData.Items[1].Quantity };
        int totalPrice = prices.Sum();
        var orderId = await _ordersRepo.CreateOrder(orderData, totalPrice);

        await Task.WhenAll(orderData.Items.Select((item, i) =>
            _orderItemsRepo.CreateOrderItem(
                item,
                orderId,
                prices[i]
            )
        ));

        return await _ordersRepo.GetOrderById(orderId);
    }
}