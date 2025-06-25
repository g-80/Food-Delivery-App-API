using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class OrdersItemsRepository : BaseRepository, IOrdersItemsRepository
{
    public OrdersItemsRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<OrderItem?> GetOrderItemById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT
                id,
                order_id,
                item_id,
                quantity,
                subtotal
            FROM order_items
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<OrderItem>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateOrderItem(CreateOrderItemDTO dto)
    {
        var parameters = new
        {
            dto.OrderId,
            dto.ItemId,
            dto.Quantity,
            dto.UnitPrice,
            dto.Subtotal,
        };
        const string sql =
            @"
            INSERT INTO order_items(order_id, item_id, quantity, unit_price, subtotal)
            VALUES
            (@OrderId, @ItemId, @Quantity, @UnitPrice, @Subtotal)
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

        const string sql =
            @"
            SELECT
                id,
                order_id,
                item_id,
                quantity,
                subtotal
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
