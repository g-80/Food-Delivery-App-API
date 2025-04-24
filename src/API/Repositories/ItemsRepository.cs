using Dapper;
using Npgsql;

public class ItemsRepository : BaseRepository, IItemsRepository
{
    public ItemsRepository(string connectionString)
        : base(connectionString) { }

    public async Task<Item?> GetItemById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT *
            FROM food_places_items
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Item>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateItem(ItemCreateRequest itemReq)
    {
        var parameters = new
        {
            itemReq.Name,
            itemReq.Description,
            itemReq.FoodPlaceId,
            itemReq.Price,
            itemReq.IsAvailable,
        };
        const string sql =
            @"
            INSERT INTO food_places_items(name, description, food_place_id, price, is_available)
            VALUES
            (@Name, @Description, @FoodPlaceId, @Price, @isAvailable)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<bool> UpdateItem(ItemUpdateRequest itemReq)
    {
        var parameters = new
        {
            itemReq.Id,
            itemReq.Name,
            itemReq.Description,
            itemReq.IsAvailable,
            itemReq.Price,
        };

        const string sql =
            @"
            UPDATE food_places_items
            SET name = @Name, description = @Description, price = @price, is_available = @IsAvailable
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
