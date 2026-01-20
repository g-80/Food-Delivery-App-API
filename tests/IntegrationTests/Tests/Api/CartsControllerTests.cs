using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Tests.Api;

public class CartsControllerTests : IntegrationTestBase
{
    private readonly DatabaseHelper _databaseHelper;

    public CartsControllerTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
        _databaseHelper = new DatabaseHelper(Fixture.PostgresConnectionString);
    }

    [Fact]
    public async Task AddItem_ValidItem_AddsToCart()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );
        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
        var command = new AddItemCommand { ItemId = foodPlaceItem.Id, Quantity = Consts.Quantities.Default };

        // Act
        var response = await Client.WithAuth(token).PostAsJsonAsync(Consts.Urls.carts, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cartItem = await _databaseHelper.GetCartItemByCustomerAndItem(
            Consts.CustomerPhoneNumber,
            foodPlaceItem.Id
        );
        cartItem.Should().NotBeNull();
        cartItem.CartId.Should().NotBe(0);
        cartItem.ItemId.Should().Be(foodPlaceItem.Id);
        cartItem.Quantity.Should().Be(Consts.Quantities.Default);
    }

    [Fact]
    public async Task UpdateQuantity_ToZero_RemovesItem()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
        await TestDataBuilder.AddTestCartItem(Consts.CustomerPhoneNumber, foodPlaceItem.Id, Consts.Quantities.Default);

        var command = new UpdateItemQuantityCommand { ItemId = foodPlaceItem.Id, Quantity = 0 };

        // Act
        var response = await Client.WithAuth(token).PatchAsJsonAsync(Consts.Urls.carts, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cartItem = await _databaseHelper.GetCartItemByCustomerAndItem(
            Consts.CustomerPhoneNumber,
            foodPlaceItem.Id
        );
        cartItem.Should().BeNull();
    }

    [Fact]
    public async Task GetCart_WithItems_ReturnsItemsAndPricing()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
        await TestDataBuilder.AddTestCartItem(Consts.CustomerPhoneNumber, foodPlaceItem.Id, Consts.Quantities.Default);

        // Act
        var response = await Client.WithAuth(token).GetAsync(Consts.Urls.carts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await response.Content.ReadFromJsonAsync<CartDTO>();
        cart.Should().NotBeNull();
        cart.Items.Should().NotBeEmpty();
        cart.Subtotal.Should().BeGreaterThan(0);
        cart.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RemoveItem_ExistingItem_RemovesFromCart()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var foodPlaceItem = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First();
        await TestDataBuilder.AddTestCartItem(Consts.CustomerPhoneNumber, foodPlaceItem.Id, Consts.Quantities.Default);
        await TestDataBuilder.AddTestCartItem(Consts.CustomerPhoneNumber, foodPlaceItem.Id, Consts.Quantities.Default);

        // Act
        var response = await Client
            .WithAuth(token)
            .DeleteAsync($"{Consts.Urls.carts}{foodPlaceItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cartItem = await _databaseHelper.GetCartItemByCustomerAndItem(
            Consts.CustomerPhoneNumber,
            foodPlaceItem.Id
        );
        cartItem.Should().BeNull();
    }
}
