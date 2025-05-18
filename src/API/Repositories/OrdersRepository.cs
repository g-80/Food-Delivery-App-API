using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class OrdersRepository : BaseRepository, IOrdersRepository
{
    public OrdersRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<Order?> GetOrderById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT *
            FROM orders
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Order>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateOrder(CreateOrderDTO dto)
    {
        var parameters = new
        {
            dto.CustomerId,
            dto.TotalPrice,
            dto.FoodPlaceId,
            dto.DeliveryAddressId,
        };
        const string sql =
            @"
            INSERT INTO orders(customer_id, food_place_id, delivery_address_id, total_price)
            VALUES
            (@CustomerId, @FoodPlaceId, @DeliveryAddressId, @TotalPrice)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<bool> CancelOrder(int id)
    {
        var parameters = new { Id = id };

        const string sql =
            @"
            UPDATE orders
            SET is_cancelled = 'true'
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            int rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        ;
    }
}
