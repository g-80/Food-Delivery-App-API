public interface ICartService
{
    Task AddItemToCartAsync(int customerId, CartAddItemRequest req);
    Task<int> CreateCartAsync(int customerId);
    Task<Cart> GetCartByCustomerIdAsync(int customerId);
    Task<CartResponse> GetCartDetailsAsync(int customerId);
    Task<IEnumerable<CartItem>> GetCartItemsByCartId(int cartId);
    Task<CartPricing?> GetCartPricingByCartId(int cartId);
    Task RemoveItemFromCartAsync(int customerId, int itemId);
    Task ResetCartAsync(int cartId);
    Task UpdateCartItemQuantityAsync(int customerId, int itemId, CartUpdateItemQuantityRequest req);
}
