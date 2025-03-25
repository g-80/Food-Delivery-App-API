using Dapper;
using Npgsql;

public class CartsRepository : BaseRepo
{
    public CartsRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Cart?> GetCartById(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
            SELECT *
            FROM carts
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Cart>(sql, parameters);
        }
        ;
    }

    public async Task<Cart?> GetCartByCustomerId(int customerId)
    {
        var parameters = new { Id = customerId };
        const string sql = @"
            SELECT *
            FROM carts
            WHERE customer_id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Cart>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateCart(CreateCartDTO dto, NpgsqlTransaction? transaction = null)
    {
        var parameters = new { dto.CustomerId, dto.Expiry };
        const string sql = @"
            INSERT INTO carts(customer_id, expires_at)
            VALUES
            (@CustomerId, @Expiry)
            RETURNING id
        ";
        if (transaction != null)
        {
            return await transaction.Connection!.ExecuteScalarAsync<int>(sql, parameters, transaction);
        }
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<int> DeleteCart(int id)
    {
        var parameters = new { Id = id };

        const string sql = @"
            DELETE FROM carts
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task<bool> SetCartAsUsed(int id, NpgsqlTransaction? transaction = null)
    {
        var parameters = new { Id = id };

        const string sql = @"
            UPDATE carts
            SET is_used = 'true'
            WHERE id = @Id
        ";
        if (transaction != null)
        {
            int rowsAffected = await transaction.Connection!.ExecuteAsync(sql, parameters, transaction);
            return rowsAffected > 0;
        }
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            int rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        ;
    }

}