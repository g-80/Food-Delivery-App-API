public class PricingService
{
    private readonly ItemsRepository _itemsRepository;
    private readonly CartItemsRepository _cartItemsRepository;

    public PricingService(ItemsRepository itemsRepository, CartItemsRepository cartItemsRepository)
    {
        _itemsRepository = itemsRepository;
        _cartItemsRepository = cartItemsRepository;
    }

    public async Task<(int, int)> CalculateItemPriceAsync(RequestedItem requestedItem)
    {
        Item item = await _itemsRepository.GetItemById(requestedItem.ItemId) ?? throw new Exception($"Item with ID: {requestedItem.ItemId} not found");
        return (item.Price, item.Price * requestedItem.Quantity);
    }

    public async Task<CartPricingDTO> CalculateCartPricing(int cartId)
    {
        IEnumerable<CartItem> cartItems = await _cartItemsRepository.GetCartItemsByCartId(cartId);
        int subtotal = cartItems.Sum(item => item.Subtotal);
        int fees = 0;
        int deliveryFee = 0;
        return new CartPricingDTO
        {
            CartId = cartId,
            Subtotal = subtotal,
            Fees = fees,
            DeliveryFee = deliveryFee,
            Total = subtotal + fees + deliveryFee
        };
    }
}