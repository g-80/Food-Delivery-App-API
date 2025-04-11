public interface ICartItemsRepository
{
    Task CreateCartItem(CreateCartItemDTO dto);
    Task DeleteCartItem(int cartId, int itemId);
    Task<CartItem?> GetCartItemById(int id);
    Task<IEnumerable<CartItem>> GetCartItemsByCartId(int cartId);
    Task UpdateCartItemQuantity(int cartId, int itemId, int quantity, int newSubtotal);
}