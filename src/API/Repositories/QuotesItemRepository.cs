using Dapper;
using Npgsql;

public class QuotesItemsRepository : BaseRepo
{
    public QuotesItemsRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<QuoteItem?> GetQuoteItemById(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
            SELECT *
            FROM quote_items
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<QuoteItem>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateQuoteItem(CreateQuoteItemDTO dto, NpgsqlTransaction? transaction = null)
    {
        var parameters = new { dto.QuoteId, dto.RequestedItem.ItemId, dto.RequestedItem.Quantity, dto.TotalPrice };
        const string sql = @"
            INSERT INTO quote_items(quote_id, item_id, quantity, total_price)
            VALUES
            (@QuoteId, @ItemId, @Quantity, @TotalPrice)
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

    public async Task<IEnumerable<QuoteItem>> GetQuoteItemsByQuoteId(int quoteId)
    {
        var parameters = new { Id = quoteId };

        const string sql = @"
            SELECT *
            FROM quote_items
            WHERE quote_id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return (await connection.QueryAsync<QuoteItem>(sql, parameters)).ToList();
        }
        ;
    }

}