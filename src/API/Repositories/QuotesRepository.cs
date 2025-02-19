using Dapper;
using Npgsql;

public class QuotesRepository : BaseRepo
{
    public QuotesRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Quote?> GetQuoteById(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
            SELECT *
            FROM quotes
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Quote>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateQuote(int customerId, int totalPrice, DateTime expiry)
    {
        var parameters = new { customerId, totalPrice, expiry };
        const string sql = @"
            INSERT INTO quotes(customer_id, price, expires_at)
            VALUES
            (@customerId, @totalPrice, @expiry)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<int> DeleteQuote(int id)
    {
        var parameters = new { Id = id };

        const string sql = @"
            DELETE FROM quotes
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task<int> SetQuoteAsUsed(int id)
    {
        var parameters = new { Id = id };

        const string sql = @"
            UPDATE quotes
            SET is_used = 'true'
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

}