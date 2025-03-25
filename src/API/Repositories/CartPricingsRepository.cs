using Dapper;
using Npgsql;

public class CartPricingsRepository : BaseRepo
{
    public CartPricingsRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<CartPricing?> GetCartPricingByCartId(int cartId)
    {
        var parameters = new { Id = cartId };
        const string sql = @"
            SELECT *
            FROM cart_pricings
            WHERE cart_id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<CartPricing>(sql, parameters);
        }
        ;
    }

    public async Task CreateCartPricing(CartPricingDTO dto, NpgsqlTransaction? transaction = null)
    {
        var parameters = new { dto.CartId, dto.Subtotal, dto.Fees, dto.DeliveryFee, dto.Total };
        const string sql = @"
            INSERT INTO cart_pricings
            VALUES
            (@CartId, @Subtotal, @Fees, @DeliveryFee, @Total)
        ";
        if (transaction != null)
        {
            await transaction.Connection!.ExecuteScalarAsync<int>(sql, parameters, transaction);
        }
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<int> DeleteCartPricing(int id)
    {
        var parameters = new { Id = id };

        const string sql = @"
            DELETE FROM cart_pricings
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task UpdateCartPricing(CartPricingDTO dto)
    {
        var parameters = dto;

        const string sql = @"
            UPDATE cart_pricings
            SET subtotal = @Subtotal, fees = @Fees, delivery_fee = @DeliveryFee, total = @Total
            WHERE cart_id = @CartId
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

}