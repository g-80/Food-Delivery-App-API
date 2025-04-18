using System.Net.Http.Json;
using FluentAssertions;

[Collection("Controllers collection")]
public class CartsControllerTests
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly CartPricingsRepository _cartPricingsRepo;
    private readonly CartService _cartService;


    public CartsControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _cartItemsRepo = _factory.GetServiceFromContainer<ICartItemsRepository>();
        _cartPricingsRepo = _factory.GetServiceFromContainer<CartPricingsRepository>();
        _cartService = _factory.GetServiceFromContainer<CartService>();
        _factory.SetCustomerAccessToken();
    }

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



