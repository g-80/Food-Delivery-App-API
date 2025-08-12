public class UpdateItemQuantityHandler
{
    private readonly ICartRepository _cartRepo;
    private readonly ILogger<UpdateItemQuantityHandler> _logger;

    public UpdateItemQuantityHandler(
        ICartRepository cartRepo,
        ILogger<UpdateItemQuantityHandler> logger
    )
    {
        _cartRepo = cartRepo;
        _logger = logger;
    }

    public async Task Handle(UpdateItemQuantityCommand req, int customerId)
    {
        var cart = await _cartRepo.GetCartByCustomerId(customerId);

        if (req.Quantity == 0)
        {
            cart.RemoveItem(req.ItemId);
        }
        else
        {
            cart.UpdateItemQuantity(req.ItemId, req.Quantity);
        }

        if (cart.IsModified)
        {
            await _cartRepo.UpdateCart(cart);
            _logger.LogInformation(
                "Item with ID: {ItemId} quantity updated to {Quantity} for customer ID: {CustomerId}.",
                req.ItemId,
                req.Quantity,
                customerId
            );
        }
    }
}
