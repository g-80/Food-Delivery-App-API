using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class CartRepository : BaseRepository, ICartRepository
{
    public CartRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task AddCart(int customerId)
    {
        var parameters = new { CustomerId = customerId };

        const string sql =
            @"
            WITH inserted_cart AS (
            INSERT INTO carts (customer_id)
            VALUES (@CustomerId)
            RETURNING id
            )
            INSERT INTO cart_pricings (cart_id, subtotal, service_fee, delivery_fee, total)
            VALUES ((SELECT id FROM inserted_cart), 0, 0, 0, 0)
        ";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
    }

    public async Task<Cart?> GetCartByCustomerId(int customerId)
    {
        var parameters = new { CustomerId = customerId };

        const string sql =
            @"
            SELECT c.id, c.customer_id, c.expires_at, fpi.food_place_id,
            c.id AS cart_id, cp.subtotal, cp.service_fee, cp.delivery_fee, cp.total,
            c.id AS cart_id, ci.item_id, ci.quantity, ci.unit_price, ci.subtotal
            FROM carts c
            INNER JOIN cart_pricings cp ON c.id = cp.cart_id
            LEFT JOIN cart_items ci ON c.id = ci.cart_id
            LEFT JOIN food_places_items fpi ON ci.item_id = fpi.id 
            WHERE c.customer_id = @CustomerId
        ";

        Cart? cart = null;
        CartPricing? pricing = null;
        var items = new List<CartItem>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.QueryAsync<QueriedCartDTO, CartPricing, CartItem, Cart?>(
                sql,
                (c, p, i) =>
                {
                    if (cart == null)
                    {
                        cart = new Cart
                        {
                            Id = c.Id,
                            CustomerId = c.CustomerId,
                            ExpiresAt = c.ExpiresAt,
                            Pricing = p,
                            FoodPlaceId = c.FoodPlaceId,
                            Items = new List<CartItem>(),
                        };
                        pricing = p;
                    }
                    // itemId = 0 means the cart is empty and it was a null in the table
                    if (i.ItemId != 0)
                    {
                        items.Add(i);
                    }
                    return null;
                },
                parameters,
                splitOn: "cart_id, cart_id"
            );
        }
        return new Cart
        {
            Id = cart!.Id,
            CustomerId = cart.CustomerId,
            ExpiresAt = cart.ExpiresAt,
            Pricing = pricing!,
            FoodPlaceId = cart.FoodPlaceId,
            Items = items,
        };
    }

    public async Task UpdateCart(Cart cart)
    {
        var parameters = new DynamicParameters();
        parameters.Add("CartId", cart.Id);
        parameters.Add("ExpiresAt", cart.ExpiresAt);
        parameters.Add("Subtotal", cart.Pricing.Subtotal);
        parameters.Add("ServiceFee", cart.Pricing.ServiceFee);
        parameters.Add("DeliveryFee", cart.Pricing.DeliveryFee);
        parameters.Add("Total", cart.Pricing.Total);

        string sql;

        if (cart.Items.Any())
        {
            parameters.Add("ItemIds", cart.Items.Select(x => x.ItemId).ToArray());
            parameters.Add("Quantities", cart.Items.Select(x => x.Quantity).ToArray());
            parameters.Add("UnitPrices", cart.Items.Select(x => x.UnitPrice).ToArray());
            parameters.Add("Subtotals", cart.Items.Select(x => x.Subtotal).ToArray());

            sql =
                @"
                INSERT INTO cart_items (cart_id, item_id, quantity, unit_price, subtotal)
                SELECT @CartId, 
                    unnest(@ItemIds::int[]), 
                    unnest(@Quantities::int[]), 
                    unnest(@UnitPrices::int[]), 
                    unnest(@Subtotals::int[])
                ON CONFLICT (cart_id, item_id) 
                DO UPDATE SET 
                    quantity = EXCLUDED.quantity,
                    unit_price = EXCLUDED.unit_price,
                    subtotal = EXCLUDED.subtotal;

                DELETE FROM cart_items 
                WHERE cart_id = @CartId AND item_id != ALL(@ItemIds);";
        }
        else
        {
            sql =
                @"
                DELETE FROM cart_items WHERE cart_id = @CartId;";
        }

        sql +=
            @"UPDATE cart_pricings
                SET subtotal = @Subtotal,
                    service_fee = @ServiceFee,
                    delivery_fee = @DeliveryFee,
                    total = @Total
                WHERE cart_id = @CartId;

                UPDATE carts 
                SET expires_at = @ExpiresAt
                WHERE id = @CartId";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    private class QueriedCartDTO
    {
        public required int Id { get; init; }
        public required int CustomerId { get; init; }
        public required DateTime ExpiresAt { get; init; }
        public required int FoodPlaceId { get; init; }
    }
}
