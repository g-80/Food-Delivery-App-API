using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class DriversRepository : BaseRepository
{
    public DriversRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<IEnumerable<AvailableDriverDTO>> GetAvailableDriversWithinDistance(
        double latitude,
        double longitude,
        int distance
    )
    {
        var parameters = new
        {
            latitude,
            longitude,
            distance,
        };

        const string sql =
            @"
        SELECT 
            u.id AS driver_id,
            u.first_name,
            u.surname,
            u.phone_number,
            ST_Distance(dl.location, ST_SetSRID(ST_Point(@longitude, @latitude), 4326)::geography) AS distance_meters
        FROM 
            users u
        INNER JOIN
            drivers_statuses ds ON u.id = ds.driver_id
        INNER JOIN 
            drivers_locations dl ON u.id = dl.driver_id
        WHERE 
            ds.status = 'online' AND
            dl.updated_at > CURRENT_TIMESTAMP - INTERVAL '2 minutes' AND
            ST_DWithin(
                dl.location, 
                ST_SetSRID(ST_Point(@longitude, @latitude), 4326)::geography,
                @distance
            )
        ORDER BY 
            distance_meters ASC
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QueryAsync<AvailableDriverDTO>(sql, parameters);
        }
        ;
    }
}
