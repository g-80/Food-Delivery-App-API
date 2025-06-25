using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class DeliveriesRepository : BaseRepository
{
    public DeliveriesRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task CreateDelivery(CreateDeliveryDTO dto)
    {
        var parameters = dto;

        const string sql =
            @"
            INSERT INTO deliveries(order_id, address_id, confirmation_code, status)
            VALUES (@OrderId, @AddressId, @ConfirmationCode, @status)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<Delivery?> GetDeliveryByOrderId(int orderId)
    {
        var parameters = new { OrderId = orderId };

        const string sql =
            @"
                SELECT *
                FROM deliveries
                WHERE order_id = @OrderId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Delivery>(sql, parameters);
        }
        ;
    }

    public async Task UpdateDeliveryStatus(int orderId, DeliveryStatuses newStatus)
    {
        var parameters = new { orderId, newStatus };

        const string sql =
            @"
                UPDATE deliveries
                SET status = @newStatus
                WHERE order_id = @orderId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task UpdateDeliveryDriver(int orderId, int driverId)
    {
        var parameters = new { orderId, driverId };

        const string sql =
            @"
                UPDATE deliveries
                SET driver_id = @driverId
                WHERE order_id = @orderId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}
