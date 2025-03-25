using System.Net.Http.Json;
using FluentAssertions;

public class CartsControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly CartItemsRepository _cartItemsRepo;
    private readonly CartPricingsRepository _cartPricingsRepo;
    private readonly CartService _cartService;

    private readonly TestDataSeeder _seeder;


    public CartsControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _cartItemsRepo = _factory.GetServiceFromContainer<CartItemsRepository>();
        _cartPricingsRepo = _factory.GetServiceFromContainer<CartPricingsRepository>();
        _cartService = _factory.GetServiceFromContainer<CartService>();
        _seeder = _factory.GetServiceFromContainer<TestDataSeeder>();
    }
    // [Fact]
    // public async Task CreateCart_ShouldReturnCartId()
    // {
    //     // Arrange
    //     var request = new AddItemToCartRequest
    //     {
    //         CustomerId = 1,
    //         Items = TestData.Orders.itemRequests
    //     };

    //     // Act
    //     var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Quotes, request);

    //     // Assert
    //     response.EnsureSuccessStatusCode();
    //     var result = await response.Content.ReadFromJsonAsync<CartResponse>();
    //     result.Should().NotBeNull();
    //     result!.QuoteId.Should().BeGreaterThan(0);
    //     result.QuoteToken.Should().NotBeNullOrWhiteSpace();

    //     // Verify the created quote in the database
    //     var repo = _factory.GetServiceFromContainer<CartsRepository>();
    //     var createdQuote = await repo.GetCartById(result.QuoteId);
    //     createdQuote.Should().NotBeNull();
    //     createdQuote!.CustomerId.Should().Be(request.CustomerId);
    //     createdQuote.Price.Should().Be(result.QuoteTokenPayload.TotalPrice);

    //     // Verify the created quote items in the database
    //     var quotesItemsRepo = _factory.GetServiceFromContainer<CartItemsRepository>();
    //     var createdQuoteItems = await quotesItemsRepo.GetCartItemsByCartId(result.QuoteId);
    //     createdQuoteItems.Should().NotBeNull();
    //     createdQuoteItems.Should().HaveCount(request.Items.Count);

    //     foreach (var item in request.Items)
    //     {
    //         var matchingItem = createdQuoteItems.Should().ContainSingle(qi => qi.ItemId == item.ItemId && qi.Quantity == item.Quantity);
    //         matchingItem.Which.QuoteId.Should().Be(result.QuoteId);
    //         matchingItem.Which.TotalPrice.Should().BeGreaterThan(0);
    //     }
    // }

    [Fact]
    public async Task AddCartitem_ShouldAddItemAndUpdateCartPricing()
    {
        // Arrange
        int cartId = TestData.Carts.assignedCartId;
        await _cartService.RemoveItemFromCartAsync(1, TestData.Carts.itemRequests[0].ItemId);
        int countBefore = (await _cartItemsRepo.GetCartItemsByCartId(cartId)).Count();
        int priceBefore = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        var request = new AddItemToCartRequest
        {
            CustomerId = 1,
            Item = TestData.Carts.itemRequests[0]
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.CartsItems, request);

        // Assert
        response.EnsureSuccessStatusCode();
        int countAfter = (await _cartItemsRepo.GetCartItemsByCartId(cartId)).Count();
        countAfter.Should().Be(countBefore + 1);
        int priceAfter = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        priceAfter.Should().BeGreaterThan(priceBefore);
    }

    [Fact]
    public async Task RemoveCartitem_ShouldRemoveItemAndUpdateCartPricing()
    {
        // Arrange
        int cartId = TestData.Carts.assignedCartId;
        int itemId = TestData.Carts.itemRequests[0].ItemId;
        int countBefore = (await _cartItemsRepo.GetCartItemsByCartId(cartId)).Count();
        int priceBefore = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;

        // Act
        var response = await _factory.Client.DeleteAsync(HttpHelper.Urls.CartsItems + itemId);

        // Assert
        response.EnsureSuccessStatusCode();
        int countAfter = (await _cartItemsRepo.GetCartItemsByCartId(cartId)).Count();
        countAfter.Should().Be(countBefore - 1);
        int priceAfter = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        priceAfter.Should().BeLessThan(priceBefore);
    }

    [Fact]
    public async Task GetCart_ShouldReturnCart_WhenCartExists()
    {
        // Arrange
        int customerId = 1;

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Carts + customerId);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCart_ShouldReturnBadRequest_WhenCartDoesNotExist()
    {
        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Carts + "9999999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_ShouldUpdateQuantityAndPricing()
    {
        // Arrange
        int cartId = TestData.Carts.assignedCartId;
        int itemId = TestData.Carts.itemRequests[1].ItemId;
        int quantityBefore = TestData.Carts.itemRequests[1].Quantity;
        int priceBefore = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        var request = new UpdateCartItemQuantityRequest
        {
            Quantity = 3
        };

        // Act
        var response = await _factory.Client.PatchAsJsonAsync(HttpHelper.Urls.CartsItems + itemId, request);

        // Assert
        response.EnsureSuccessStatusCode();
        int quantityAfter = (await _cartItemsRepo.GetCartItemsByCartId(cartId)).First(item => item.ItemId == itemId).Quantity;
        quantityAfter.Should().NotBe(quantityBefore);
        int priceAfter = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        priceAfter.Should().NotBe(priceBefore);
    }
}



