using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class DriversStatusesRepository : BaseRepository
{
    public DriversStatusesRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task CreateDriverStatus(int driverId, string status)
    {
        var parameters = new { Id = driverId, Status = status };

        const string sql =
            @"
            INSERT INTO drivers_statuses (driver_id, status)
            VALUES (@Id, @Status)
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task UpdateDriverStatus(int driverId, string newStatus)
    {
        var parameters = new { Id = driverId, Status = newStatus };

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

    public async Task RemoveDriverStatus(int driverId)
    {
        var parameters = new { Id = driverId };

        const string sql =
            @"
            DELETE FROM drivers_statuses
            WHERE driver_id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }
}
