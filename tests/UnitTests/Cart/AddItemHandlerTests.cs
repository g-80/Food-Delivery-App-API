using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.CartTests;

public class AddItemHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IFoodPlaceRepository> _foodPlaceRepoMock;
    private readonly Mock<ILogger<AddItemHandler>> _loggerMock;
    private readonly AddItemHandler _handler;
    private readonly Cart _cart = new()
    {
        Id = 1,
        CustomerId = 1,
        ExpiresAt = DateTime.UtcNow.AddMinutes(15),
        Items = new List<CartItem>(),
        Pricing = new CartPricing()
        {
            CartId = 1,
            Subtotal = 0,
            ServiceFee = 0,
            DeliveryFee = 0,
            Total = 0,
        },
    };
    private readonly FoodPlace _foodPlace = new()
    {
        Id = 1,
        Name = "Test Food Place",
        Description = "A test food place",
        Category = "Test Category",
        AddressId = 1,
        Location = new Location { Latitude = 0, Longitude = 0 },
        Items = new List<FoodPlaceItem>
        {
            new FoodPlaceItem
            {
                Id = 1,
                Name = "Test Item",
                Price = 100,
                IsAvailable = true,
            },
        },
    };

    public AddItemHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _foodPlaceRepoMock = new Mock<IFoodPlaceRepository>();
        _loggerMock = new Mock<ILogger<AddItemHandler>>();
        _handler = new AddItemHandler(
            _cartRepoMock.Object,
            _foodPlaceRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldAddItemToCart()
    {
        // Arrange
        int itemId = 1;

        _foodPlaceRepoMock
            .Setup(repo => repo.GetFoodPlaceByItemId(itemId))
            .ReturnsAsync(_foodPlace);

        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        var req = new AddItemCommand { ItemId = itemId, Quantity = 2 };

        // Act
        await _handler.Handle(req, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(_cart), Times.Once);
        _cart.Items.Should().ContainSingle(i => i.ItemId == itemId && i.Quantity == req.Quantity);
    }

    [Fact]
    public async Task Handle_ShouldUpdateQuantityIfItemAlreadyExists()
    {
        // Arrange
        int itemId = 1;

        _foodPlaceRepoMock
            .Setup(repo => repo.GetFoodPlaceByItemId(itemId))
            .ReturnsAsync(_foodPlace);
        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        var existingItem = new CartItem
        {
            ItemId = itemId,
            CartId = _cart.Id,
            Quantity = 1,
            UnitPrice = 100,
            Subtotal = 100,
        };
        _cart.AddItem(existingItem, _foodPlace.Id);

        var req = new AddItemCommand { ItemId = itemId, Quantity = 3 };

        // Act
        await _handler.Handle(req, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(_cart), Times.Once);
        _cart.Items.Should().ContainSingle(i => i.ItemId == itemId && i.Quantity == 4);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenFoodPlaceNotFound()
    {
        // Arrange
        int itemId = 1;
        _foodPlaceRepoMock
            .Setup(repo => repo.GetFoodPlaceByItemId(itemId))
            .ReturnsAsync((FoodPlace)null);
        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        var req = new AddItemCommand { ItemId = itemId, Quantity = 2 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(req, _cart.CustomerId)
        );
    }

    [Fact]
    public async Task Handle_ShouldClearCartIfItemIsFromADifferentFoodPlace()
    {
        // Arrange
        int itemId = 2;
        var differentFoodPlace = new FoodPlace
        {
            Id = 2,
            Name = "Different Food Place",
            Description = "A different food place",
            Category = "Different Category",
            AddressId = 2,
            Location = new Location { Latitude = 1, Longitude = 1 },
            Items = new List<FoodPlaceItem>
            {
                new FoodPlaceItem
                {
                    Id = itemId,
                    Name = "Test Item",
                    Price = 100,
                    IsAvailable = true,
                },
            },
        };

        _foodPlaceRepoMock
            .Setup(repo => repo.GetFoodPlaceByItemId(itemId))
            .ReturnsAsync(differentFoodPlace);
        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        var req = new AddItemCommand { ItemId = itemId, Quantity = 2 };

        // Act
        await _handler.Handle(req, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(_cart), Times.Once);
        _cart.Items.Should().ContainSingle(i => i.ItemId == itemId && i.Quantity == req.Quantity);
        _cart.Items.Should().NotContain(i => i.ItemId != itemId);
    }
}
