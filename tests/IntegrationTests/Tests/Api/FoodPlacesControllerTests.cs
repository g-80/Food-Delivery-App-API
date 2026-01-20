using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Tests.Api;

public class FoodPlacesControllerTests : IntegrationTestBase
{
    private readonly DatabaseHelper _databaseHelper;

    public FoodPlacesControllerTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
        _databaseHelper = new DatabaseHelper(fixture.PostgresConnectionString);
    }

    [Fact]
    public async Task GetNearbyFoodPlaces_ValidLocationWithNearbyPlaces_ReturnsOk()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        // Act
        var response = await Client
            .WithAuth(token)
            .GetAsync(
                $"{Consts.Urls.foodPlacesNearby}?latitude={Consts.LondonCentralLat}&longitude={Consts.LondonCentralLong}"
            );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetNearbyFoodPlaces_ValidLocationNoNearbyPlaces_ReturnsEmptyArray()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        // Act
        var response = await Client
            .WithAuth(token)
            .GetAsync(
                $"{Consts.Urls.foodPlacesNearby}?latitude={Consts.GlasgowLat}&longitude={Consts.GlasgowLong}"
            );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Italian")]
    [InlineData("Test")]
    [InlineData("%20Italian%20")]
    [InlineData("itALIaN")]
    public async Task SearchFoodPlaces_ValidQueryMatchesCategory_ReturnsMatchingPlaces(string query)
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        // Act
        var response = await Client
            .WithAuth(token)
            .GetAsync(
                $"{Consts.Urls.foodPlacesSearch}?latitude={Consts.LondonCentralLat}&longitude={Consts.LondonCentralLong}&searchQuery={query}"
            );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchFoodPlaces_ValidQueryNoMatches_ReturnsEmptyArray()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        // Act
        var response = await Client
            .WithAuth(token)
            .GetAsync(
                $"{Consts.Urls.foodPlacesSearch}?latitude={Consts.LondonCentralLat}&longitude={Consts.LondonCentralLong}&searchQuery=NonExistentRestaurant123"
            );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFoodPlace_ValidExistingId_ReturnsFoodPlaceWithItems()
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

        // Act
        var response = await Client
            .WithAuth(token)
            .GetAsync($"{Consts.Urls.foodPlaces}{foodPlaceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<FoodPlaceDTO>();
        result.Should().NotBeNull();
        result.Id.Should().NotBe(0);
        result.Name.Should().NotBeNull();
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFoodPlace_NonExistentId_Returns404()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.CustomerPhoneNumber,
            Consts.TestPassword,
            Client
        );

        // Act
        var response = await Client.WithAuth(token).GetAsync($"{Consts.Urls.foodPlaces}99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateFoodPlace_ValidRequest_CreatesPlaceAndReturns200()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.FoodPlacePhoneNumber,
            Consts.TestPassword,
            Client
        );

        var command = new
        {
            Name = "Test Restaurant",
            Description = "A test place",
            Category = "Italian",
            Latitude = Consts.LondonCentralLat,
            Longitude = Consts.LondonCentralLong,
            Address = new
            {
                NumberAndStreet = Consts.Addresses.Street,
                City = Consts.Addresses.City,
                Postcode = Consts.Addresses.Postcode,
            },
        };

        // Act
        var response = await Client
            .WithAuth(token)
            .PostAsJsonAsync(Consts.Urls.foodPlaces, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdId = await response.Content.ReadFromJsonAsync<int>();
        createdId.Should().BeGreaterThan(0);

        var foodPlace = await _databaseHelper.GetFoodPlaceById(createdId);
        foodPlace.Should().NotBeNull();
        foodPlace.Name.Should().Be("Test Restaurant");
        foodPlace.Category.Should().Be("Italian");
    }

    [Fact]
    public async Task CreateItem_ValidRequest_CreatesItemAndReturns200()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.FoodPlacePhoneNumber,
            Consts.TestPassword,
            Client
        );

        var command = new
        {
            Name = "New Pizza",
            Description = "Delicious pizza",
            Price = 1500,
            IsAvailable = true,
        };

        // Act
        var response = await Client
            .WithAuth(token)
            .PostAsJsonAsync(Consts.Urls.foodPlacesItems, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var item = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First(item =>
            item.Name == "New Pizza"
        );
        item.Should().NotBeNull();
        item.Price.Should().Be(1500);
    }

    [Fact]
    public async Task UpdateItem_ValidRequest_UpdatesItemAndReturns200()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.FoodPlacePhoneNumber,
            Consts.TestPassword,
            Client
        );

        var foodPlaceId = await _databaseHelper.GetFoodPlaceIdByPhoneNumber(
            Consts.FoodPlacePhoneNumber
        );
        var itemId = (await _databaseHelper.GetFoodPlaceItems(foodPlaceId)).First().Id;

        var command = new
        {
            Id = itemId,
            Name = "Updated Name",
            Description = "Updated description",
            Price = 2000,
            IsAvailable = false,
        };

        // Act
        var response = await Client
            .WithAuth(token)
            .PutAsJsonAsync(Consts.Urls.foodPlacesItems, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var item = await _databaseHelper.GetFoodPlaceItemById(itemId);
        item.Should().NotBeNull();
        item.Name.Should().Be("Updated Name");
        item.Price.Should().Be(2000);
        item.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateItem_NonExistentItemId_ReturnsError()
    {
        // Arrange
        var token = await AuthHelper.LogInUser(
            Consts.FoodPlacePhoneNumber,
            Consts.TestPassword,
            Client
        );

        var command = new
        {
            Id = 99999,
            Name = "Test",
            Price = 1000,
            IsAvailable = true,
        };

        // Act
        var response = await Client
            .WithAuth(token)
            .PutAsJsonAsync(Consts.Urls.foodPlacesItems, command);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}
