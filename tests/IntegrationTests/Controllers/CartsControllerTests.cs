using System.Net.Http.Json;
using System.Transactions;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Npgsql;

[Collection("Controllers collection")]
public class CartsControllerTests
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly ICartPricingsRepository _cartPricingsRepo;
    private readonly ICartService _cartService;
    private readonly ICartsRepository _cartsRepo;

    public CartsControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _cartItemsRepo = _factory.GetServiceFromContainer<ICartItemsRepository>();
        _cartPricingsRepo = _factory.GetServiceFromContainer<ICartPricingsRepository>();
        _cartService = _factory.GetServiceFromContainer<ICartService>();
        _cartsRepo = _factory.GetServiceFromContainer<ICartsRepository>();
    }

    [Fact]
    public async Task AddCartitem_ShouldAddItemAndUpdateCartPricing_WhenItemNotInCart()
    {
        // Arrange
        int cartId = TestData.Carts.assignedCartId;
        int customerId = TestData.Users.assignedIds[0];
        // remove item from cart to add it again
        await _cartService.RemoveItemFromCartAsync(
            customerId,
            TestData.Carts.itemRequests[0].ItemId
        );
        int countBefore = (await _cartItemsRepo.GetCartItemsByCartId(cartId)).Count();
        int priceBefore = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        var request = new CartAddItemRequest { Item = TestData.Carts.itemRequests[0] };

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
    public async Task AddCartitem_ShouldUpdateQuantityAndPrice_WhenItemAlreadyExists()
    {
        // Arrange
        int cartId = TestData.Carts.assignedCartId;
        int itemId = TestData.Carts.itemRequests[0].ItemId;

        var existingItems = await _cartItemsRepo.GetCartItemsByCartId(cartId);
        var existingItem = existingItems.First(i => i.ItemId == itemId);

        int quantityBefore = existingItem.Quantity;
        int priceBefore = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        int countBefore = existingItems.Count();

        var request = new CartAddItemRequest
        {
            Item = new RequestedItem { ItemId = itemId, Quantity = 1 },
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.CartsItems, request);

        // Assert
        response.EnsureSuccessStatusCode();

        var itemsAfter = await _cartItemsRepo.GetCartItemsByCartId(cartId);
        itemsAfter.Count().Should().Be(countBefore);

        var updatedItem = itemsAfter.First(i => i.ItemId == itemId);
        updatedItem.Quantity.Should().Be(quantityBefore + 1);

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

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Carts);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCart_ShouldRefreshExpiry_WhenCartIsExpired()
    {
        // Arrange
        int customerId = TestData.Users.assignedIds[0];

        // Get current cart
        var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

        // Manually expire the cart by setting expiry to past
        DateTime expiredTime = DateTime.UtcNow.AddMinutes(-10);
        await _cartsRepo.UpdateCartExpiry(cart.Id, expiredTime);

        cart = await _cartsRepo.GetCartById(cart.Id);
        cart!.ExpiresAt.Should().BeBefore(DateTime.UtcNow);

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Carts);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify cart expiry was refreshed
        var refreshedCart = await _cartsRepo.GetCartById(cart.Id);
        refreshedCart!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateCartItemQuantity_ShouldUpdateQuantityAndPricing()
    {
        // Arrange
        int cartId = TestData.Carts.assignedCartId;
        int itemId = TestData.Carts.itemRequests[1].ItemId;
        int quantityBefore = TestData.Carts.itemRequests[1].Quantity;
        int priceBefore = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        var request = new CartUpdateItemQuantityRequest { Quantity = 3 };

        using var testScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        // Act
        var response = await _factory.Client.PatchAsJsonAsync(
            HttpHelper.Urls.CartsItems + itemId,
            request
        );

        // Assert
        response.EnsureSuccessStatusCode();
        int quantityAfter = (await _cartItemsRepo.GetCartItemsByCartId(cartId))
            .First(item => item.ItemId == itemId)
            .Quantity;
        quantityAfter.Should().NotBe(quantityBefore);
        int priceAfter = (await _cartPricingsRepo.GetCartPricingByCartId(cartId))!.Total;
        priceAfter.Should().NotBe(priceBefore);
    }
}
