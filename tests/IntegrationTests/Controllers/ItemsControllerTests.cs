using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

[Collection("Controllers collection")]
public class ItemsControllerTests
{
    private readonly WebApplicationFactoryFixture _factory;

    public ItemsControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OnCreateItem_ShouldCreateItemAndReturnId()
    {
        // Arrange
        var itemReq = new ItemCreateRequest
        {
            Name = "Vegetarian Pizza",
            FoodPlaceId = 1,
            Price = 450,
            IsAvailable = true,
            Description = "Delicious veggie pizza",
        };
        await _factory.LoginAsAFoodPlace();

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        var createdId = await response.Content.ReadFromJsonAsync<int>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        createdId.Should().BeGreaterThan(0);

        // Verify the created item
        var repo = _factory.GetServiceFromContainer<IItemsRepository>();
        var createdItem = await repo.GetItemById(createdId);
        createdItem.Should().NotBeNull();
        createdItem!.Name.Should().Be(itemReq.Name);
        createdItem.Description.Should().Be(itemReq.Description);
        createdItem.Price.Should().Be(itemReq.Price);
        createdItem.FoodPlaceId.Should().Be(itemReq.FoodPlaceId);
        createdItem.IsAvailable.Should().Be(itemReq.IsAvailable);
    }

    [Theory]
    [InlineData("Ab")]
    [InlineData("ThisNameIsWayTooLongForTheMaximumLength")]
    [InlineData("")]
    public async Task OnCreateItem_WithInvalidName_ShouldReturnBadRequest(string name)
    {
        // Arrange
        var itemReq = new ItemCreateRequest
        {
            Name = name,
            FoodPlaceId = 1,
            Price = 450,
            IsAvailable = true,
        };
        await _factory.LoginAsAFoodPlace();

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors!.Errors.Should().ContainKey("Name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task OnCreateItem_WithInvalidPrice_ShouldReturnBadRequest(int price)
    {
        // Arrange
        var itemReq = new ItemCreateRequest
        {
            Name = "Amazing Pizza",
            FoodPlaceId = 1,
            Price = price,
            IsAvailable = true,
        };
        await _factory.LoginAsAFoodPlace();

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors!.Errors.Should().ContainKey("Price");
    }

    [Fact]
    public async Task OnGetItem_ShouldReturnExpectedItem()
    {
        // Arrange
        int id = TestData.Items.assignedIds[0];
        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Items + id);
        var result = await response.Content.ReadFromJsonAsync<Item>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().NotBeNull();
        var testItem = TestData.Items.defaults[0];
        result!.Name.Should().Be(testItem.Name);
        result.Price.Should().Be(testItem.Price);
    }

    [Fact]
    public async Task OnGetNonExistentItem_ShouldReturnNotFound()
    {
        // Arrange

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Items + "999999");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OnUpdateItem_ShouldUpdateItemAndReturnUpdated()
    {
        // Arrange
        var testItem = TestData.Items.defaults[0];
        int id = TestData.Items.assignedIds[0];
        testItem.IsAvailable.Should().BeTrue();
        ItemUpdateRequest itemReq = new ItemUpdateRequest
        {
            Name = testItem.Name,
            Id = id,
            IsAvailable = false,
            Price = testItem.Price,
        };
        await _factory.LoginAsAFoodPlace();

        // Act
        var response = await _factory.Client.PutAsJsonAsync(HttpHelper.Urls.Items + id, itemReq);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var repo = _factory.GetServiceFromContainer<IItemsRepository>();
        Item? after = await repo.GetItemById(id);
        after!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task OnUpdateInvalidItem_ShouldReturnBadRequest()
    {
        // Arrange
        var testItem = TestData.Items.defaults[0];
        int id = TestData.Items.assignedIds[0];
        testItem.IsAvailable.Should().BeTrue();
        testItem.Price.Should().Be(750);
        ItemUpdateRequest itemReq = new ItemUpdateRequest
        {
            Name = testItem.Name,
            Id = id,
            IsAvailable = testItem.IsAvailable,
            Price = -750,
        };
        await _factory.LoginAsAFoodPlace();

        // Act
        var response = await _factory.Client.PutAsJsonAsync(HttpHelper.Urls.Items + id, itemReq);
        var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors!.Errors.Should().ContainKey("Price");
    }
}
