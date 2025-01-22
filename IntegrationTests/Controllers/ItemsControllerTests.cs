using System.Net.Http.Json;
using FluentAssertions;

public class ItemsControllerTests : IClassFixture<WebApplicationFactoryFixture>
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
        CreateItemRequest itemReq = new CreateItemRequest { Name = "Vegetarian Pizza", FoodPlaceId = 1, Price = 450 };
        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        var result = await response.Content.ReadFromJsonAsync<int>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().NotBe(0);
    }

    [Fact]
    public async Task OnCreateInvalidItem_ShouldReturnBadRequest()
    {
        // Arrange
        CreateItemRequest itemReq = new CreateItemRequest { Name = "Vegetarian Pizza", FoodPlaceId = 1, Price = -500 };
        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OnGetItem_ShouldReturnExpectedItem()
    {
        // Arrange

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Items + "1");
        var result = await response.Content.ReadFromJsonAsync<Item>();
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().NotBe(null);
    }

    [Fact]
    public async Task OnGetNonExistentItem_ShouldReturnNotFound()
    {
        // Arrange

        // Act
        var response = await _factory.Client.GetAsync(HttpHelper.Urls.Items + "9999");
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OnUpdateItem_ShouldUpdateItemAndReturnUpdated()
    {
        // Arrange
        var repo = _factory.GetRepoFromServices<ItemsRepository>();
        Item before = await repo.GetItemById(2);
        before.IsAvailable.Should().BeTrue();
        UpdateItemRequest itemReq = new UpdateItemRequest { Name = before.Name, Id = before.Id, IsAvailable = false, Price = before.Price };
        // Act
        var response = await _factory.Client.PutAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        Item after = await repo.GetItemById(2);
        after.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task OnUpdateInvalidItem_ShouldReturnBadRequest()
    {
        // Arrange
        var repo = _factory.GetRepoFromServices<ItemsRepository>();
        Item before = await repo.GetItemById(2);
        UpdateItemRequest itemReq = new UpdateItemRequest { Name = before.Name, Id = before.Id, IsAvailable = before.IsAvailable, Price = -450 };
        // Act
        var response = await _factory.Client.PutAsJsonAsync(HttpHelper.Urls.Items, itemReq);
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

}