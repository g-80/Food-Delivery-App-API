using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class DriversLocationsRepository : BaseRepository
{
    public DriversLocationsRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task UpsertDriverLocation(int driverId, double latitude, double longitude)
    {
        var parameters = new
        {
            Id = driverId,
            Lat = latitude,
            Long = longitude,
        };

        const string sql =
            @"
            INSERT INTO drivers_locations
            (driver_id, location)
            VALUES
            (@Id, ST_SETSRID(ST_MakePoint(@Long, @Lat), 4326))
            ON CONFLICT (driver_id)
            DO UPDATE SET
            location = EXCLUDED.location
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task RemoveDriverLocation(int driverId)
    {
        var parameters = new { Id = driverId };

        const string sql =
            @"
            DELETE FROM drivers_locations
            WHERE driver_id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}
