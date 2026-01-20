using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class FoodPlaceRepository : BaseRepository, IFoodPlaceRepository
{
    public FoodPlaceRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<int> AddFoodPlace(FoodPlace foodPlace, int userId)
    {
        var parameters = new
        {
            foodPlace.Name,
            foodPlace.Description,
            foodPlace.Category,
            foodPlace.AddressId,
            foodPlace.Location.Latitude,
            foodPlace.Location.Longitude,
            UserId = userId,
        };
        const string sql =
            @"
            WITH inserted_location AS (
                INSERT INTO food_places_locations (location)
                VALUES (ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326))
                RETURNING id
            )
            INSERT INTO food_places (name, description, category, address_id, location_id, user_id)
            VALUES (@Name, @Description, @Category, @AddressId, (SELECT id FROM inserted_location), @UserId)
            RETURNING id";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
    }

    public async Task AddFoodPlaceItem(int foodPlaceId, FoodPlaceItem item)
    {
        var parameters = new
        {
            FoodPlaceId = foodPlaceId,
            item.Name,
            item.Description,
            item.Price,
            item.IsAvailable,
        };
        const string sql =
            @"
            INSERT INTO food_places_items (food_place_id, name, description, price, is_available)
            VALUES (@FoodPlaceId, @Name, @Description, @Price, @IsAvailable)";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task<FoodPlace?> GetFoodPlaceById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT fp.id, fp.name, fp.description, fp.category, fp.address_id,
            ST_Y(fpl.location) AS latitude,
            ST_X(fpl.location) AS longitude,
            fpi.id, fpi.name, fpi.description, fpi.price, fpi.is_available, fpi.created_at
            FROM food_places fp
            INNER JOIN food_places_locations fpl ON fp.location_id = fpl.id
            LEFT JOIN food_places_items fpi ON fp.id = fpi.food_place_id
            WHERE fp.id = @Id";

        FoodPlace? foodPlace = null;
        List<FoodPlaceItem> items = new();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            // when food place item is null, it gets cast to FoodPlaceItem, but the properties are required(not null). whats returned in this case?
            await connection.QueryAsync<QueriedFoodPlaceDTO, Location, FoodPlaceItem, FoodPlace?>(
                sql,
                (fp, loc, fpi) =>
                {
                    if (foodPlace == null)
                    {
                        foodPlace = new FoodPlace
                        {
                            Id = fp.Id,
                            Name = fp.Name,
                            Description = fp.Description,
                            Category = fp.Category,
                            AddressId = fp.AddressId,
                            Location = new Location
                            {
                                Latitude = loc.Latitude,
                                Longitude = loc.Longitude,
                            },
                            Items = items,
                        };
                    }
                    items.Add(fpi);
                    return null;
                },
                parameters,
                splitOn: "latitude, id"
            );
        }
        if (foodPlace == null)
        {
            return null;
        }
        return new FoodPlace
        {
            Id = foodPlace.Id,
            Name = foodPlace.Name,
            Description = foodPlace.Description,
            Category = foodPlace.Category,
            AddressId = foodPlace.AddressId,
            Location = foodPlace.Location,
            Items = items,
        };
    }

    public async Task<FoodPlace?> GetFoodPlaceByItemId(int itemId)
    {
        var parameters = new { ItemId = itemId };
        const string sql =
            @"
            SELECT fp.id, fp.name, fp.description, fp.category, fp.address_id,
            ST_Y(fpl.location) AS latitude,
            ST_X(fpl.location) AS longitude,
            fpi.id, fpi.name, fpi.description, fpi.price, fpi.is_available, fpi.created_at
            FROM food_places fp
            INNER JOIN food_places_locations fpl ON fp.location_id = fpl.id
            INNER JOIN food_places_items fpi ON fp.id = fpi.food_place_id
            WHERE fp.id = (
                SELECT food_place_id FROM food_places_items WHERE id = @ItemId
            )";

        FoodPlace? foodPlace = null;
        List<FoodPlaceItem> items = new();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.QueryAsync<QueriedFoodPlaceDTO, Location, FoodPlaceItem, FoodPlace?>(
                sql,
                (fp, loc, fpi) =>
                {
                    if (foodPlace == null)
                    {
                        foodPlace = new FoodPlace
                        {
                            Id = fp.Id,
                            Name = fp.Name,
                            Description = fp.Description,
                            Category = fp.Category,
                            AddressId = fp.AddressId,
                            Location = new Location
                            {
                                Latitude = loc.Latitude,
                                Longitude = loc.Longitude,
                            },
                            Items = items,
                        };
                    }
                    items.Add(fpi);
                    return null;
                },
                parameters,
                splitOn: "latitude, id"
            );
        }
        return new FoodPlace
        {
            Id = foodPlace!.Id,
            Name = foodPlace.Name,
            Description = foodPlace.Description,
            Category = foodPlace.Category,
            AddressId = foodPlace.AddressId,
            Location = foodPlace.Location,
            Items = items,
        };
    }

    public async Task<FoodPlace> GetFoodPlaceByUserId(int userId)
    {
        var parameters = new { UserId = userId };
        const string sql =
            @"
            SELECT fp.id, fp.name, fp.description, fp.category, fp.address_id,
            ST_Y(fpl.location) AS latitude,
            ST_X(fpl.location) AS longitude,
            fpi.id, fpi.name, fpi.description, fpi.price, fpi.is_available, fpi.created_at
            FROM food_places fp
            INNER JOIN food_places_locations fpl ON fp.location_id = fpl.id
            LEFT JOIN food_places_items fpi ON fp.id = fpi.food_place_id
            WHERE fp.user_id = @UserId";

        FoodPlace? foodPlace = null;
        List<FoodPlaceItem> items = new();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.QueryAsync<QueriedFoodPlaceDTO, Location, FoodPlaceItem, FoodPlace?>(
                sql,
                (fp, loc, fpi) =>
                {
                    if (foodPlace == null)
                    {
                        foodPlace = new FoodPlace
                        {
                            Id = fp.Id,
                            Name = fp.Name,
                            Description = fp.Description,
                            Category = fp.Category,
                            AddressId = fp.AddressId,
                            Location = new Location
                            {
                                Latitude = loc.Latitude,
                                Longitude = loc.Longitude,
                            },
                            Items = items,
                        };
                    }
                    items.Add(fpi);
                    return null;
                },
                parameters,
                splitOn: "latitude, id"
            );
        }
        return new FoodPlace
        {
            Id = foodPlace!.Id,
            Name = foodPlace.Name,
            Description = foodPlace.Description,
            Category = foodPlace.Category,
            AddressId = foodPlace.AddressId,
            Location = foodPlace.Location,
            Items = items,
        };
    }

    public async Task<int> GetFoodPlaceUserId(int foodPlaceId)
    {
        var parameters = new { FoodPlaceId = foodPlaceId };
        const string sql =
            @"
            SELECT user_id FROM food_places WHERE id = @FoodPlaceId";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
    }

    public async Task<IEnumerable<FoodPlace>> GetNearbyFoodPlaces(
        GetNearbyFoodPlacesQuery query,
        int distanceMeters
    )
    {
        var parameters = new
        {
            query.Latitude,
            query.Longitude,
            Distance = distanceMeters,
        };
        const string sql =
            @"
            SELECT fp.id, fp.name, fp.description, fp.category, fp.address_id,
            ST_Y(fpl.location) AS latitude,
            ST_X(fpl.location) AS longitude,
            ST_Distance(fpl.location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography) AS distance
            FROM food_places fp
            INNER JOIN food_places_locations fpl ON fp.location_id = fpl.id
            WHERE ST_DWithin(fpl.location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography, @Distance)
            ORDER BY distance
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var result = await connection.QueryAsync<QueriedFoodPlaceDTO, Location, FoodPlace>(
                sql,
                (foodPlace, location) =>
                {
                    return new FoodPlace
                    {
                        Id = foodPlace.Id,
                        Name = foodPlace.Name,
                        Description = foodPlace.Description,
                        Category = foodPlace.Category,
                        AddressId = foodPlace.AddressId,
                        Location = location,
                    };
                },
                parameters,
                splitOn: "latitude"
            );
            return result.ToList();
        }
        ;
    }

    public async Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(
        SearchFoodPlacesQuery query,
        int distanceMeters = 3000
    )
    {
        var parameters = new
        {
            query.Latitude,
            query.Longitude,
            query.SearchQuery,
            Distance = distanceMeters,
        };
        const string sql =
            @"
            SELECT fp.id, fp.name, fp.description, fp.category, fp.address_id,
            ST_Y(fpl.location) AS latitude,
            ST_X(fpl.location) AS longitude,
            ST_Distance(fpl.location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography) AS distance
            FROM food_places fp
            INNER JOIN food_places_locations fpl ON fp.location_id = fpl.id
            WHERE search_vector @@ plainto_tsquery('english', @SearchQuery)
            AND ST_DWithin(fpl.location::geography, ST_POINT(@Longitude, @Latitude, 4326)::geography, @Distance)
            ORDER BY distance
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var result = await connection.QueryAsync<QueriedFoodPlaceDTO, Location, FoodPlace>(
                sql,
                (foodPlace, location) =>
                {
                    return new FoodPlace
                    {
                        Id = foodPlace.Id,
                        Name = foodPlace.Name,
                        Description = foodPlace.Description,
                        Category = foodPlace.Category,
                        AddressId = foodPlace.AddressId,
                        Location = location,
                    };
                },
                parameters,
                splitOn: "latitude"
            );
            return result.ToList();
        }
        ;
    }

    public async Task UpdateFoodPlace(FoodPlace foodPlace)
    {
        var parameters = new
        {
            foodPlace.Id,
            foodPlace.Name,
            foodPlace.Description,
            foodPlace.Category,
        };
        const string sql =
            @"
            UPDATE FoodPlace 
            SET name = @Name, description = @Description, category = @Category
            WHERE Id = @Id 
            AND (name != @Name OR description != @Description OR category != @Category)";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task UpdateFoodPlaceItem(FoodPlaceItem item)
    {
        var parameters = new
        {
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.IsAvailable,
        };
        const string sql =
            @"
            UPDATE food_places_items
            SET name = @Name,
                description = @Description,
                price = @Price,
                is_available = @IsAvailable
            WHERE id = @Id
            AND (
                name IS DISTINCT FROM @Name OR
                description IS DISTINCT FROM @Description OR
                price IS DISTINCT FROM @Price OR
                is_available IS DISTINCT FROM @IsAvailable
            )";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    private class QueriedFoodPlaceDTO
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required string? Description { get; init; }
        public required string Category { get; init; }
        public required int AddressId { get; init; }
    }
}
