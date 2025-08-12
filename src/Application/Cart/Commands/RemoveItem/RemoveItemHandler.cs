public class RemoveItemHandler
{
    private readonly ICartRepository _cartRepo;
    private readonly ILogger<RemoveItemHandler> _logger;

    public RemoveItemHandler(ICartRepository cartRepo, ILogger<RemoveItemHandler> logger)
    {
        _cartRepo = cartRepo;
        _logger = logger;
    }

    public async Task Handle(int itemId, int customerId)
    {
        var cart = await _cartRepo.GetCartByCustomerId(customerId);

        cart.RemoveItem(itemId);

        await _cartRepo.UpdateCart(cart);
        _logger.LogInformation(
            "Item with ID: {ItemId} removed from cart for customer ID: {CustomerId}.",
            itemId,
            customerId
        );
    }
}
