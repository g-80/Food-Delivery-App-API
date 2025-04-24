using Dapper;
using Npgsql;

public class CartItemsRepository : BaseRepository, ICartItemsRepository
{
    public CartItemsRepository(string connectionString)
        : base(connectionString) { }

    public async Task CreateCartItem(CreateCartItemDTO dto)
    {
        var parameters = new
        {
            dto.CartId,
            dto.RequestedItem.ItemId,
            dto.RequestedItem.Quantity,
            dto.UnitPrice,
            dto.Subtotal,
        };
        const string sql =
            @"
            INSERT INTO cart_items(cart_id, item_id, quantity, unit_price, subtotal)
            VALUES
            (@CartId, @ItemId, @Quantity, @UnitPrice, @Subtotal)
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsByCartId(int cartId)
    {
        var parameters = new { Id = cartId };

        const string sql =
            @"
            SELECT *
            FROM cart_items
            WHERE cart_id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QueryAsync<CartItem>(sql, parameters);
        }
        ;
    }

    public async Task DeleteCartItem(int cartId, int itemId)
    {
        var parameters = new { cartId, itemId };

        const string sql =
            @"
            DELETE FROM cart_items
            WHERE cart_id = @cartId AND item_id = @itemId
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task UpdateCartItemQuantity(int cartId, int itemId, int quantity, int newSubtotal)
    {
        var parameters = new
        {
            cartId,
            itemId,
            quantity,
            newSubtotal,
        };

        const string sql =
            @"
            UPDATE cart_items
            SET quantity = @quantity, subtotal = @newSubtotal
            WHERE cart_id = @cartId AND item_id = @itemId
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task DeleteAllCartItemsByCartId(int cartId)
    {
        var parameters = new { cartId };
        const string sql =
            @"
            DELETE FROM cart_items
            WHERE cart_id = @cartId
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task UpdateCartItemPrice(int cartId, int itemId, int newUnitPrice, int newSubtotal)
    {
        var parameters = new
        {
            cartId,
            itemId,
            newUnitPrice,
            newSubtotal,
        };

        const string sql =
            @"
            UPDATE cart_items
            SET unit_price = @newUnitPrice, subtotal = @newSubtotal
            WHERE cart_id = @cartId AND item_id = @itemId
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }
}
