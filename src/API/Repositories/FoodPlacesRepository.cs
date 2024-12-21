using Npgsql;
using Dapper;

public class FoodPlacesRepository : BaseRepo
{
    const int DEFAULT_DISTANCE = 3000;
    public FoodPlacesRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<FoodPlace>> GetFoodPlacesWithinDistance(NearbyFoodPlacesRequest query)
    {
        var parameters = new { query.Latitude, query.Longitude, Distance = DEFAULT_DISTANCE };
        const string sql = @"
            SELECT
                id,
                name,
                description,
                category,
                latitude,
                longitude,
                ST_Distance(location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography) AS distance
            FROM food_places
            WHERE ST_DWithin(location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography, @Distance)
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return (await connection.QueryAsync<FoodPlace>(sql, parameters)).ToList();
        };
    }

    public async Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(SearchFoodPlacesRequest query)
    {
        var parameters = new { query.Latitude, query.Longitude, Distance = DEFAULT_DISTANCE, query.SearchQuery };
        const string sql = @"
            SELECT
                id,
                name,
                description,
                category,
                latitude,
                longitude,
                ST_Distance(location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography) AS distance
            FROM food_places
            WHERE search_vector @@ plainto_tsquery('english', @SearchQuery)
            AND ST_DWithin(location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography, @Distance)
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return (await connection.QueryAsync<FoodPlace>(sql, parameters)).ToList();
        };
    }
    public async Task<FoodPlace?> GetFoodPlace(int id)
    {
        var parameters = new { Id = id };
        const string sql = @"
            SELECT
                id,
                name,
                description,
                category,
                latitude,
                longitude
            FROM food_places
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<FoodPlace>(sql, parameters);
        };
    }
}