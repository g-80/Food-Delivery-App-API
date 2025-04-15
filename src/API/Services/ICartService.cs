public interface ICartService
{
    Task AddItemToCartAsync(AddItemToCartRequest req);
    Task<int> CreateCartAsync(int customerId);
    Task<Cart> GetCartByCustomerIdAsync(int customerId);
    Task<CartResponse> GetCartDetailsAsync(int customerId);
    Task RemoveItemFromCartAsync(int customerId, int itemId);
    Task ResetCartAsync(int cartId);
    Task UpdateCartItemQuantityAsync(int customerId, int itemId, UpdateCartItemQuantityRequest req);
}
