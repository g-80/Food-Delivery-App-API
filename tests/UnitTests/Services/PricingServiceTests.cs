using FluentAssertions;
using Moq;

public class PricingServiceTests
{
    private readonly Mock<ItemsRepository> _itemsRepositoryMock;
    private readonly Mock<CartItemsRepository> _cartItemsRepositoryMock;
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        _itemsRepositoryMock = new Mock<ItemsRepository>("TestConnectionString");
        _cartItemsRepositoryMock = new Mock<CartItemsRepository>("TestConnectionString");
        _pricingService = new PricingService(_itemsRepositoryMock.Object, _cartItemsRepositoryMock.Object);
    }

    [Fact]
    public async Task CalculateCartPricingAsync_Should_ReturnCorrectTotals()
    {
        // Arrange
        IEnumerable<CartItem> items = new List<CartItem>
        {
            new() { ItemId = 1, Quantity = 2, UnitPrice = 250, Subtotal = 500 },
            new() { ItemId = 2, Quantity = 3, UnitPrice = 300, Subtotal = 900 }
        };


        _cartItemsRepositoryMock.Setup(repo => repo.GetCartItemsByCartId(1))
            .ReturnsAsync(items);

        // Act
        var pricing = await _pricingService.CalculateCartPricing(1);

        // Assert
        pricing.Total.Should().BeGreaterThanOrEqualTo(1400);
    }

    [Fact]
    public async Task CalculatePriceAsync_ShouldThrowException_IfItemNotFound()
    {
        // Arrange
        int id = 99;
        var item = new RequestedItem { ItemId = id, Quantity = 1 };

        _itemsRepositoryMock.Setup(repo => repo.GetItemById(id))
            .ReturnsAsync((Item)null); // Simulating item not found

        // Act
        Func<Task> act = async () => await _pricingService.CalculateItemPriceAsync(item);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
