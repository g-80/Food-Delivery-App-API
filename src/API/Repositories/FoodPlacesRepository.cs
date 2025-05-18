using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class FoodPlacesRepository : BaseRepository, IFoodPlacesRepository
{
    private readonly int _distanceMeters = 3000;

    public FoodPlacesRepository(IOptions<DatabaseOptions> options, int distance)
        : base(options.Value.ConnectionString)
    {
        _distanceMeters = distance;
    }

    public async Task<IEnumerable<FoodPlace>> GetFoodPlacesWithinDistance(
        FoodPlacesNearbyRequest query
    )
    {
        var parameters = new
        {
            query.Latitude,
            query.Longitude,
            Distance = _distanceMeters,
        };
        const string sql =
            @"
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
        }
        ;
    }

    public async Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(
        FoodPlacesSearchRequest query
    )
    {
        var parameters = new
        {
            query.Latitude,
            query.Longitude,
            query.SearchQuery,
            Distance = _distanceMeters,
        };
        const string sql =
            @"
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
        }
        ;
    }

    public async Task<FoodPlace?> GetFoodPlace(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT
                id,
                name,
                description,
                category,
                latitude,
                longitude,
                address_id
            FROM food_places
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<FoodPlace>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateFoodPlace(FoodPlaceCreateRequest request)
    {
        var parameters = new
        {
            request.Name,
            request.Description,
            request.Category,
            request.Latitude,
            request.Longitude,
        };
        const string sql =
            @"
            INSERT INTO food_places(name, description, category, latitude, longitude)
            VALUES
            (@Name, @Description, @Category, @Latitude, @Longitude)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }
}
