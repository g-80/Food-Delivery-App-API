using FluentAssertions;
using Moq;

public class PricingServiceTests
{
    private readonly Mock<ItemsRepository> _itemsRepositoryMock;
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        _itemsRepositoryMock = new Mock<ItemsRepository>("TestConnectionString");
        _pricingService = new PricingService(_itemsRepositoryMock.Object);
    }

    [Fact]
    public async Task CalculatePriceAsync_Should_Return_Correct_Totals()
    {
        // Arrange
        var items = new List<ItemRequest>
        {
            new() { ItemId = 1, Quantity = 2 },
            new() { ItemId = 2, Quantity = 3 }
        };


        _itemsRepositoryMock.Setup(repo => repo.GetItemById(1))
            .ReturnsAsync(new Item { Id = 1, Price = 250 });

        _itemsRepositoryMock.Setup(repo => repo.GetItemById(2))
            .ReturnsAsync(new Item { Id = 2, Price = 300 });

        // Act
        var (prices, totalPrice) = await _pricingService.CalculatePriceAsync(items);

        // Assert
        prices.Should().Equal(new List<int> { 500, 900 });
        totalPrice.Should().Be(1400);
    }

    [Fact]
    public async Task CalculatePriceAsync_Should_Throw_Exception_If_Item_Not_Found()
    {
        // Arrange
        int id = 99;
        var items = new List<ItemRequest> { new() { ItemId = id, Quantity = 1 } };


        _itemsRepositoryMock.Setup(repo => repo.GetItemById(id))
            .ReturnsAsync((Item)null); // Simulating item not found

        // Act
        Func<Task> act = async () => await _pricingService.CalculatePriceAsync(items);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Item with ID: {id} not found");
    }
}
