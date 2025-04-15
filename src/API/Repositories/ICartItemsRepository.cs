using Npgsql;

public interface ICartItemsRepository
{
    Task CreateCartItem(CreateCartItemDTO dto);
    Task DeleteAllCartItemsByCartId(int cartId, NpgsqlTransaction? transaction = null);
    Task DeleteCartItem(int cartId, int itemId);
    Task<IEnumerable<CartItem>> GetCartItemsByCartId(int cartId);
    Task UpdateCartItemQuantity(int cartId, int itemId, int quantity, int newSubtotal);
}
