using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class CartsRepository : BaseRepository, ICartsRepository
{
    public CartsRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<Cart?> GetCartById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
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
        const string sql =
            @"
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

    public async Task<int> CreateCart(CreateCartDTO dto)
    {
        var parameters = new { dto.CustomerId, dto.Expiry };
        const string sql =
            @"
            INSERT INTO carts(customer_id, expires_at)
            VALUES
            (@CustomerId, @Expiry)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task UpdateCartExpiry(int cartId, DateTime newExpiry)
    {
        var parameters = new { Id = cartId, Expiry = newExpiry };

        const string sql =
            @"
            UPDATE carts
            SET expires_at = @Expiry
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}
