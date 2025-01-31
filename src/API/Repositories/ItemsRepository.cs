using Dapper;
using Npgsql;

public class ItemsRepository : BaseRepo
{
    public ItemsRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Item?> GetItemById(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
            SELECT id, name, description, food_place_id AS FoodPlaceId, created_at AS CreatedAt, is_available AS IsAvailable, price
            FROM food_places_items
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Item>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateItem(CreateItemRequest itemReq)
    {
        var parameters = new { itemReq.Name, itemReq.Description, itemReq.FoodPlaceId, itemReq.Price, itemReq.IsAvailable };
        const string sql = @"
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

    public async Task<int> UpdateItem(UpdateItemRequest itemReq)
    {
        var parameters = new { itemReq.Id, itemReq.Name, itemReq.Description, itemReq.IsAvailable, itemReq.Price };

        const string sql = @"
            UPDATE food_places_items
            SET name = @Name, description = @Description, price = @price, is_available = @IsAvailable
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}

// TODO:
// add try catch to ensure null returns getting handled
// update is returning id/n of rows changed. fix it to return the updated row