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
            INSERT INTO deliveries(order_id, address_id, driver_id, confirmation_code, status)
            VALUES (@OrderId, @AddressId, @DriverId, @ConfirmationCode, @status)
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

    public async Task SetAsDelivered(int orderId)
    {
        var parameters = new { OrderId = orderId };

        const string sql =
            @"
                UPDATE deliveries
                SET is_delivered = 'true'
                WHERE order_id = @OrderId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}
