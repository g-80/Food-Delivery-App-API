using Npgsql;

public interface ICartItemsRepository
{
    Task CreateCartItem(CreateCartItemDTO dto);
    Task DeleteAllCartItemsByCartId(int cartId);
    Task DeleteCartItem(int cartId, int itemId);
    Task<IEnumerable<CartItem>> GetCartItemsByCartId(int cartId);
    Task UpdateCartItemPrice(int cartId, int itemId, int newUnitPrice, int newSubtotal);
    Task UpdateCartItemQuantity(int cartId, int itemId, int quantity, int newSubtotal);
}
