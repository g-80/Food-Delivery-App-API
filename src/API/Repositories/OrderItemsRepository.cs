using Dapper;
using Npgsql;

public class OrderItemsRepository : BaseRepo
{
    public OrderItemsRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<OrderItem?> GetOrderItemById(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
            SELECT
                id,
                order_id,
                item_id,
                quantity,
                total_price
            FROM order_items
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<OrderItem>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateOrderItem(ItemRequest orderItemReq, int orderId, int totalPrice)
    {
        var parameters = new { orderId, orderItemReq.ItemId, orderItemReq.Quantity, totalPrice };
        const string sql = @"
            INSERT INTO order_items(order_id, item_id, quantity, total_price)
            VALUES
            (@orderId, @ItemId, @Quantity, @totalPrice)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(int orderId)
    {
        var parameters = new { Id = orderId };

        const string sql = @"
            SELECT
                id,
                order_id,
                item_id,
                quantity,
                total_price
            FROM order_items
            WHERE order_id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return (await connection.QueryAsync<OrderItem>(sql, parameters)).ToList();
        }
        ;
    }

}