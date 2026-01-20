using Dapper;
using Npgsql;

namespace IntegrationTests.Helpers;

public record DriverLocationHistoryRecord
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    public DateTime Timestamp { get; set; }
    public int? DeliveryId { get; set; }
}

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetUserByPhoneNumber(string phoneNumber)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE phone_number = @PhoneNumber",
            new { PhoneNumber = phoneNumber }
        );
    }

    public async Task<int> GetUserIdByPhoneNumber(string phoneNumber)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QuerySingleAsync<int>(
            "SELECT id FROM users WHERE phone_number = @PhoneNumber",
            new { PhoneNumber = phoneNumber }
        );
    }

    public async Task<int> GetFoodPlaceIdByPhoneNumber(string phoneNumber)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QuerySingleAsync<int>(
            @"SELECT fp.id FROM food_places fp
            INNER JOIN users u on fp.user_id = u.id
            WHERE phone_number = @PhoneNumber",
            new { PhoneNumber = phoneNumber }
        );
    }

    public async Task<List<DriverLocationHistoryRecord>> GetLocationHistory(
        int driverId,
        int deliveryId
    )
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        var results = await conn.QueryAsync<DriverLocationHistoryRecord>(
            @"SELECT id, driver_id, latitude, longitude, accuracy, speed, heading, timestamp, delivery_id
              FROM driver_location_history
              WHERE driver_id = @DriverId AND delivery_id = @DeliveryId
              ORDER BY timestamp DESC",
            new { DriverId = driverId, DeliveryId = deliveryId }
        );
        return results.ToList();
    }

    public async Task<int> CreateDelivery(
        int orderId,
        DeliveryStatuses status,
        int? driverId = null
    )
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO deliveries (order_id, driver_id, status, confirmation_code)
              VALUES (@OrderId, @DriverId, @Status, '1234')
              RETURNING id",
            new
            {
                OrderId = orderId,
                DriverId = driverId,
                Status = (int)status,
            }
        );
    }

    public async Task<int> CreateOrder(int customerId, int foodPlaceId, OrderStatuses status)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var addressId = await conn.QuerySingleAsync<int>(
            "SELECT id FROM addresses WHERE user_id = @CustomerId LIMIT 1",
            new { CustomerId = customerId }
        );

        var orderId = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO orders (customer_id, food_place_id, delivery_address_id, status, service_fee, delivery_fee)
              VALUES (@CustomerId, @FoodPlaceId, @AddressId, @Status, @ServiceFee, @DeliveryFee)
              RETURNING id",
            new
            {
                CustomerId = customerId,
                FoodPlaceId = foodPlaceId,
                AddressId = addressId,
                Status = (int)status,
                ServiceFee = Consts.Prices.ServiceFee,
                DeliveryFee = Consts.Prices.DeliveryFee,
            }
        );

        return orderId;
    }

    public async Task<Delivery?> GetDeliveryByOrderId(int orderId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<Delivery>(
            "SELECT *, route AS route_json FROM deliveries WHERE order_id = @OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<Order?> GetOrderById(int orderId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM orders WHERE id = @OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task CreatePayment(
        int orderId,
        int amount,
        PaymentStatuses status,
        string stripePaymentIntentId
    )
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO payments (order_id, stripe_payment_intent_id, amount, status, created_at)
              VALUES (@OrderId, @StripePaymentIntentId, @Amount, @Status, CURRENT_TIMESTAMP)",
            new
            {
                OrderId = orderId,
                Amount = amount,
                Status = (int)status,
                StripePaymentIntentId = stripePaymentIntentId,
            }
        );
    }

    public async Task<Payment?> GetPaymentByOrderId(int orderId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<Payment>(
            "SELECT * FROM payments WHERE order_id = @OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task UpdatePaymentIntentId(int orderId, string newStripePaymentIntentId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "UPDATE payments SET stripe_payment_intent_id = @IntentId WHERE order_id = @OrderId",
            new { IntentId = newStripePaymentIntentId, OrderId = orderId }
        );
    }

    public async Task<int> GetCartItemCount(int customerId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM cart_items ci
              INNER JOIN carts c ON ci.cart_id = c.id
              WHERE c.customer_id = @CustomerId",
            new { CustomerId = customerId }
        );
    }

    public async Task<IEnumerable<FoodPlaceItem>> GetFoodPlaceItems(int foodPlaceId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryAsync<FoodPlaceItem>(
            "SELECT * FROM food_places_items WHERE food_place_id = @FoodPlaceId",
            new { FoodPlaceId = foodPlaceId }
        );
    }

    public async Task<CartItem?> GetCartItemByCustomerAndItem(string phoneNumber, int itemId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<CartItem>(
            @"SELECT ci.* FROM cart_items ci
              JOIN carts c ON c.id = ci.cart_id
              JOIN users u ON u.id = c.customer_id
              WHERE u.phone_number = @PhoneNumber AND ci.item_id = @ItemId",
            new { PhoneNumber = phoneNumber, ItemId = itemId }
        );
    }

    public async Task<FoodPlace?> GetFoodPlaceById(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<FoodPlace>(
            "SELECT * FROM food_places WHERE id = @Id",
            new { Id = id }
        );
    }

    public async Task<FoodPlaceItem?> GetFoodPlaceItemById(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<FoodPlaceItem>(
            "SELECT * FROM food_places_items WHERE id = @Id",
            new { Id = id }
        );
    }

    public async Task<int> GetLocationHistoryCount(int driverId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM driver_location_history WHERE driver_id = @DriverId",
            new { DriverId = driverId }
        );
    }

    public async Task<Cart?> GetCartByCustomerId(int customerId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<Cart>(
            "SELECT * FROM carts WHERE customer_id = @CustomerId",
            new { CustomerId = customerId }
        );
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        var results = await conn.QueryAsync<OrderItem>(
            "SELECT order_id, item_id, quantity, unit_price FROM order_items WHERE order_id = @OrderId",
            new { OrderId = orderId }
        );
        return results.ToList();
    }

    public async Task CreateOrderItemForOrder(int orderId, int itemId, int quantity, int unitPrice)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO order_items (order_id, item_id, quantity, unit_price)
              VALUES (@OrderId, @ItemId, @Quantity, @UnitPrice)",
            new
            {
                OrderId = orderId,
                ItemId = itemId,
                Quantity = quantity,
                UnitPrice = unitPrice,
            }
        );
    }
}
