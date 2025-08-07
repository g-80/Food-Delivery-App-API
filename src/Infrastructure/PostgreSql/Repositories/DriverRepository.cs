using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class DriverRepository : BaseRepository, IDriverRepository
{
    public DriverRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<bool> ConnectDriver(int driverId, DriverStatuses status)
    {
        var parameters = new { Id = driverId, Status = status };

        const string sql =
            @"
            INSERT INTO drivers_statuses
            (driver_id, status)
            VALUES
            (@Id, @Status)
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters) > 0;
        }
        ;
    }

    public async Task DisconnectDriver(int driverId)
    {
        var parameters = new { Id = driverId };

        const string sql =
            @"
            DELETE FROM drivers_statuses
            WHERE driver_id = @Id;
            DELETE FROM drivers_locations
            WHERE driver_id = @Id;
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task<IEnumerable<AvailableDriver>> GetAvailableDriversWithinDistance(
        double latitude,
        double longitude,
        int distance,
        DriverStatuses status
    )
    {
        var parameters = new
        {
            latitude,
            longitude,
            distance,
            status,
        };

        const string sql =
            @"
        SELECT 
            u.id,
            ST_Distance(dl.location, ST_SetSRID(ST_Point(@longitude, @latitude), 4326)::geography) AS distance
        FROM 
            users u
        INNER JOIN
            drivers_statuses ds ON u.id = ds.driver_id
        INNER JOIN 
            drivers_locations dl ON u.id = dl.driver_id
        WHERE 
            ds.status = @status AND
            dl.updated_at > CURRENT_TIMESTAMP - INTERVAL '2 minutes' AND
            ST_DWithin(
                dl.location, 
                ST_SetSRID(ST_Point(@longitude, @latitude), 4326)::geography,
                @distance
            )
        ORDER BY 
            distance ASC
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QueryAsync<AvailableDriver>(sql, parameters);
        }
        ;
    }

    public async Task<Driver?> GetDriverById(int id)
    {
        var parameters = new { Id = id };

        const string sql =
            @"
        SELECT
            ds.driver_id AS Id,
            ds.status,
            ST_Y(dl.location) AS Latitude,
            ST_X(dl.location) AS Longitude
        FROM
            drivers_statuses ds
        LEFT JOIN
            drivers_locations dl ON ds.driver_id = dl.driver_id
        WHERE
            ds.driver_id = @Id
        ";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync<Driver, Location, Driver>(
                sql,
                (driver, loc) =>
                {
                    driver.Location = loc;
                    return driver;
                },
                parameters,
                splitOn: "Latitude"
            );
            return result.FirstOrDefault();
        }
    }

    public async Task UpdateDriverStatus(Driver driver)
    {
        var parameters = new { driver.Id, driver.Status };

        const string sql =
            @"
            UPDATE drivers_statuses
            SET status = @Status
            WHERE driver_id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task UpsertDriverLocation(Driver driver)
    {
        var parameters = new
        {
            driver.Id,
            Lat = driver.Location!.Latitude,
            Long = driver.Location.Longitude,
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
}
