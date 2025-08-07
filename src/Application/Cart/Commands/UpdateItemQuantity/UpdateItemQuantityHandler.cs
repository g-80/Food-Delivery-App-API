public class UpdateItemQuantityHandler
{
    private readonly ICartRepository _cartRepo;

    public UpdateItemQuantityHandler(ICartRepository cartRepo)
    {
        _cartRepo = cartRepo;
    }

    public async Task Handle(UpdateItemQuantityCommand req, int customerId)
    {
        var cart =
            await _cartRepo.GetCartByCustomerId(customerId)
            ?? throw new Exception($"Cart for customer ID: {customerId} not found.");

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
        }
    }
}
