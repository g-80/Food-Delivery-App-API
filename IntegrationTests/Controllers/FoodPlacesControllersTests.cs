using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;

public class FoodPlacesControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;

    public FoodPlacesControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OnGetFoodPlace_ShouldReturnExpectedFoodPlace()
    {
        // Arrange

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.GetFoodPlace + "1");
        var result = await response.Content.ReadFromJsonAsync<FoodPlace>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().NotBe(null);
    }

    [Fact]
    public async Task OnGetNonExistentFoodPlace_ShouldReturnNotFound()
    {
        // Arrange

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.GetFoodPlace + "9999");
        var result = await response.Content.ReadFromJsonAsync<FoodPlace>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        result.Id.Should().Be(0);
    }

    [Fact]
    public async Task OnSearchFoodPlaces_ShouldReturnFoodPlaces()
    {
        // Arrange
        string searchQuery = "greek";
        var query = new Dictionary<string, string>
        {
            ["latitude"] = $"{FoodPlacesFixtures.locationLatLong.Item1}",
            ["longitude"] = $"{FoodPlacesFixtures.locationLatLong.Item2}",
            ["searchquery"] = searchQuery
        };
        // Act
        var response = await _factory.Client.GetAsync(QueryHelpers.AddQueryString(HttpHelper.Urls.SearchFoodPlaces, query));
        var result = await response.Content.ReadFromJsonAsync<List<FoodPlace>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().AllSatisfy(foodPlace => foodPlace.Category.ToLower().Should().Contain(searchQuery));
    }

    [Fact]
    public async Task OnGetNearbyFoodPlaces_ShouldReturnFoodPlaces()
    {
        // Arrange
        var query = new Dictionary<string, string>
        {
            ["latitude"] = $"{FoodPlacesFixtures.locationLatLong.Item1}",
            ["longitude"] = $"{FoodPlacesFixtures.locationLatLong.Item2}",
        };
        // Act
        var response = await _factory.Client.GetAsync(QueryHelpers.AddQueryString(HttpHelper.Urls.GetNearbyFoodPlaces, query));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task OnGetNearbyFoodPlacesWithInvalidLatLong_ShouldReturnBadRequest()
    {
        // Arrange
        var query = new Dictionary<string, string>
        {
            ["latitude"] = "53.964945",
            ["longitude"] = "3.761067",
        };
        // Act
        var response = await _factory.Client.GetAsync(QueryHelpers.AddQueryString(HttpHelper.Urls.GetNearbyFoodPlaces, query));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}