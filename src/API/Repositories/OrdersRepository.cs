using Dapper;
using Npgsql;

public class OrdersRepository : BaseRepo
{
    public OrdersRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Order?> GetOrderById(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
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

    public async Task<int> CreateOrder(CustomerItemsRequest req, int totalPrice)
    {
        var parameters = new { req.CustomerId, totalPrice };
        const string sql = @"
            INSERT INTO orders(customer_id, total_price)
            VALUES
            (@customerId, @totalPrice)
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

        const string sql = @"
            UPDATE orders
            SET is_cancelled = 'true'
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            int nRows = await connection.ExecuteAsync(sql, parameters);
            if (nRows == 0)
            {
                return false;
            }
            return true;
        }
        ;
    }

}
