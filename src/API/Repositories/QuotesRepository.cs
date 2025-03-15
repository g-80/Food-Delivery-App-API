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

    public async Task<int> CreateQuote(CreateQuoteDTO dto, NpgsqlTransaction? transaction = null)
    {
        var parameters = new { dto.CustomerId, dto.TotalPrice, dto.Expiry };
        const string sql = @"
            INSERT INTO quotes(customer_id, price, expires_at)
            VALUES
            (@CustomerId, @TotalPrice, @Expiry)
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

    public async Task<bool> SetQuoteAsUsed(int id, NpgsqlTransaction? transaction = null)
    {
        var parameters = new { Id = id };

        const string sql = @"
            UPDATE quotes
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