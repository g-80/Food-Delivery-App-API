public class RemoveItemHandler
{
    private readonly ICartRepository _cartRepo;

    public RemoveItemHandler(ICartRepository cartRepo)
    {
        _cartRepo = cartRepo;
    }

    public async Task Handle(int itemId, int customerId)
    {
        var cart =
            await _cartRepo.GetCartByCustomerId(customerId)
            ?? throw new Exception($"Cart for customer ID: {customerId} not found.");

        cart.RemoveItem(itemId);

        await _cartRepo.UpdateCart(cart);
    }
}
