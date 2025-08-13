using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.CartTests;

public class RemoveItemHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<ILogger<RemoveItemHandler>> _loggerMock;
    private readonly RemoveItemHandler _handler;
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

    public RemoveItemHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _loggerMock = new Mock<ILogger<RemoveItemHandler>>();
        _handler = new RemoveItemHandler(_cartRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItemFromCart()
    {
        // Arrange
        int itemId = 1;
        var item = new CartItem
        {
            ItemId = itemId,
            CartId = 1,
            Quantity = 1,
            UnitPrice = 100,
            Subtotal = 100,
        };
        _cart.AddItem(item, 1);

        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        // Act
        await _handler.Handle(itemId, _cart.CustomerId);

        // Assert
        _cartRepoMock.Verify(repo => repo.UpdateCart(_cart), Times.Once);
        _cart.Items.Should().NotContain(i => i.ItemId == itemId);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenItemNotFound()
    {
        // Arrange
        int itemId = 1;
        _cartRepoMock.Setup(repo => repo.GetCartByCustomerId(_cart.CustomerId)).ReturnsAsync(_cart);

        // Act
        Func<Task> act = async () => await _handler.Handle(itemId, _cart.CustomerId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
