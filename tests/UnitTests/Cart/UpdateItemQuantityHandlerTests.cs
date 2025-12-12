using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.CartTests;

public class UpdateItemQuantityHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<ILogger<UpdateItemQuantityHandler>> _loggerMock;
    private readonly UpdateItemQuantityHandler _handler;
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
        },
    };

    public UpdateItemQuantityHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _loggerMock = new Mock<ILogger<UpdateItemQuantityHandler>>();
        _handler = new UpdateItemQuantityHandler(_cartRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateItemQuantityInCart()
    {
        // Arrange
        int itemId = 1;
        var item = new CartItem
        {
            ItemId = itemId,
            CartId = 1,
            Quantity = 1,
            UnitPrice = 100,
        };
        _cart.AddItem(item, 1);

        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);
        var req = new UpdateItemQuantityCommand { ItemId = itemId, Quantity = 3 };

        // Act
        await _handler.Handle(req, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(_cart), Times.Once);
        _cart.Items.Should().ContainSingle(i => i.ItemId == itemId && i.Quantity == req.Quantity);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItemWhenQuantityIsZero()
    {
        // Arrange
        int itemId = 1;
        var item = new CartItem
        {
            ItemId = itemId,
            CartId = 1,
            Quantity = 1,
            UnitPrice = 100,
        };
        _cart.AddItem(item, 1);

        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);
        var req = new UpdateItemQuantityCommand { ItemId = itemId, Quantity = 0 };

        // Act
        await _handler.Handle(req, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(_cart), Times.Once);
        _cart.Items.Should().NotContain(i => i.ItemId == itemId);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateCartIfNoChangesInItemQuantity()
    {
        // Arrange
        int itemId = 1;
        var item = new CartItem
        {
            ItemId = itemId,
            CartId = 1,
            Quantity = 1,
            UnitPrice = 100,
        };
        _cart.AddItem(item, 1);

        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);
        var req = new UpdateItemQuantityCommand { ItemId = itemId, Quantity = 1 };

        // Act
        await _handler.Handle(req, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenItemNotFound()
    {
        // Arrange
        var req = new UpdateItemQuantityCommand { ItemId = 3, Quantity = 2 };
        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        // Act
        Func<Task> act = async () => await _handler.Handle(req, _cart.CustomerId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
