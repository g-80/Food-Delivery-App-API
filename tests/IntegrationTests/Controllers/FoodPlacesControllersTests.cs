using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
        var fixtures = Fixtures.foodPlacesFixtures;
        int seededDataId = 1;
        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.GetFoodPlace + seededDataId);
        var result = await response.Content.ReadFromJsonAsync<FoodPlace>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Id.Should().Be(seededDataId);
        result.Name.Should().Be(fixtures[0].Name);
        result.Category.Should().Be(fixtures[0].Category);
    }

    [Fact]
    public async Task OnGetNonExistentFoodPlace_ShouldReturnNotFound()
    {
        // Arrange
        int fixturesCount = Fixtures.foodPlacesFixtures.Count;
        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.GetFoodPlace + (fixturesCount + 1));
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    public async Task OnGetFoodPlace_WithInvalidId_ShouldReturnBadRequest(string invalidId)
    {
        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.GetFoodPlace + invalidId);
        var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors.Extensions.Should().ContainKey("id");
    }

    [Fact]
    public async Task OnSearchFoodPlaces_ShouldReturnFoodPlaces()
    {
        // Arrange
        string searchQuery = "greek";
        var query = new Dictionary<string, string>
        {
            ["latitude"] = $"{Fixtures.locationLatLong.Item1}",
            ["longitude"] = $"{Fixtures.locationLatLong.Item2}",
            ["searchquery"] = searchQuery
        };
        // Act
        var response = await _factory.Client.GetAsync(QueryHelpers.AddQueryString(HttpHelper.Urls.SearchFoodPlaces, query));
        var result = await response.Content.ReadFromJsonAsync<List<FoodPlace>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().AllSatisfy(foodPlace => foodPlace.Category.ToLower().Should().Contain(searchQuery));
    }

    [Theory]
    [InlineData("")]
    [InlineData("gr")]
    [InlineData("AWaaaaaaaaaaaaaaaaaaaaaaaaaaaaaayTooLongSearchQuery")]
    public async Task OnSearchFoodPlaces_WithInvalidSearchQuery_ShouldReturnBadRequest(string searchQuery)
    {
        // Arrange
        var query = new Dictionary<string, string>
        {
            ["latitude"] = $"{Fixtures.locationLatLong.Item1}",
            ["longitude"] = $"{Fixtures.locationLatLong.Item2}",
            ["searchquery"] = searchQuery
        };
        // Act
        var response = await _factory.Client.GetAsync(QueryHelpers.AddQueryString(HttpHelper.Urls.SearchFoodPlaces, query));
        var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors.Errors.Should().ContainKey("SearchQuery");

    }

    [Fact]
    public async Task OnGetNearbyFoodPlaces_ShouldReturnFoodPlaces()
    {
        // Arrange
        var query = new Dictionary<string, string>
        {
            ["latitude"] = $"{Fixtures.locationLatLong.Item1}",
            ["longitude"] = $"{Fixtures.locationLatLong.Item2}",
        };
        // Act
        var response = await _factory.Client.GetAsync(QueryHelpers.AddQueryString(HttpHelper.Urls.GetNearbyFoodPlaces, query));
        var result = await response.Content.ReadFromJsonAsync<List<FoodPlace>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Count.Should().Be(Fixtures.foodPlacesFixtures.Count);
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
        var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        (errors.Errors.ContainsKey("Latitude") || errors.Errors.ContainsKey("Longitude")).Should().BeTrue();
    }
}