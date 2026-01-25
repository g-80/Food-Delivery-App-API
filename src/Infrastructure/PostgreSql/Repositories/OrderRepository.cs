using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class OrderRepository : BaseRepository, IOrderRepository
{
    public OrderRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<int> AddOrder(Order order)
    {
        var parameters = new DynamicParameters();
        parameters.Add("CustomerId", order.CustomerId);
        parameters.Add("FoodPlaceId", order.FoodPlaceId);
        parameters.Add("DeliveryAddressId", order.DeliveryAddressId);
        parameters.Add("ServiceFee", order.ServiceFee);
        parameters.Add("DeliveryFee", order.DeliveryFee);
        parameters.Add("Status", order.Status);
        parameters.Add("CreatedAt", order.CreatedAt);
        parameters.Add("ItemIds", order.Items.Select(x => x.ItemId).ToArray());
        parameters.Add("Quantities", order.Items.Select(x => x.Quantity).ToArray());
        parameters.Add("UnitPrices", order.Items.Select(x => x.UnitPrice).ToArray());

        var sql =
            @"
        WITH inserted_order AS (
            INSERT INTO orders(customer_id, food_place_id, delivery_address_id, service_fee, delivery_fee, status, created_at)
            VALUES
            (@CustomerId, @FoodPlaceId, @DeliveryAddressId, @ServiceFee, @DeliveryFee, @Status, @CreatedAt)
            RETURNING id
        )
        INSERT INTO order_items (order_id, item_id, quantity, unit_price)
        SELECT
            (SELECT id FROM inserted_order),
            unnest(@ItemIds::int[]),
            unnest(@Quantities::int[]),
            unnest(@UnitPrices::int[])
        RETURNING (SELECT id FROM inserted_order);
        ";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            // open manually to allow the connection to enlist in the ambient transaction
            await connection.OpenAsync();
            return await connection.ExecuteScalarAsync<int>(sql.ToString(), parameters);
        }
    }

    public async Task<IEnumerable<Order>> GetAllOrdersByCustomerId(int customerId)
    {
        var parameters = new { CustomerId = customerId };
        const string sql =
            @"
            SELECT
            o.id, o.customer_id, o.food_place_id, o.delivery_address_id, o.service_fee, o.delivery_fee, o.status, o.created_at,
            oi.item_id, oi.quantity, oi.unit_price
            FROM orders o
            INNER JOIN order_items oi ON o.id = oi.order_id
            WHERE o.customer_id = @CustomerId
            ORDER BY o.created_at DESC
            ";

        var orderDictionary = new Dictionary<int, Order>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.QueryAsync<QueriedOrderDTO, OrderItem, Order?>(
                sql,
                (order, item) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                    {
                        orderDictionary.Add(
                            order.Id,
                            new Order
                            {
                                Id = order.Id,
                                CustomerId = order.CustomerId,
                                FoodPlaceId = order.FoodPlaceId,
                                DeliveryAddressId = order.DeliveryAddressId,
                                ServiceFee = order.ServiceFee,
                                DeliveryFee = order.DeliveryFee,
                                Status = order.Status,
                                CreatedAt = order.CreatedAt,
                                Items = new List<OrderItem>() { item }.AsReadOnly(),
                            }
                        );
                    }
                    else
                    {
                        currentOrder = new Order
                        {
                            Id = order.Id,
                            CustomerId = order.CustomerId,
                            FoodPlaceId = order.FoodPlaceId,
                            DeliveryAddressId = order.DeliveryAddressId,
                            ServiceFee = order.ServiceFee,
                            DeliveryFee = order.DeliveryFee,
                            Status = order.Status,
                            CreatedAt = order.CreatedAt,
                            Items = new List<OrderItem>(currentOrder.Items) { item }.AsReadOnly(),
                        };
                    }

                    return null;
                },
                parameters,
                splitOn: "item_id"
            );
        }
        ;
        return orderDictionary.Values;
    }

    public async Task<Order?> GetOrderById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT
            o.id, o.customer_id, o.food_place_id, o.delivery_address_id, o.service_fee, o.delivery_fee, o.status, o.created_at,
            oi.item_id, oi.quantity, oi.unit_price,
            d.id, d.driver_id, d.confirmation_code, d.status, d.delivered_at, d.route, d.payment_amount,
            p.amount, p.stripe_payment_intent_id, p.status
            FROM orders o
            INNER JOIN order_items oi ON o.id = oi.order_id
            LEFT JOIN deliveries d ON o.id = d.order_id
            INNER JOIN payments p ON o.id = p.order_id
            WHERE o.id = @Id
            ";

        Order? result = null;
        List<OrderItem> items = new();
        Delivery? delivery = null;
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            // <type to map the first part to, type to map the second part to, return type of the lambda>
            await connection.QueryAsync<
                QueriedOrderDTO,
                OrderItem,
                QueriedDeliveryDTO,
                Payment,
                Order?
            >(
                sql,
                (order, item, deliveryDto, payment) =>
                {
                    // This will run for each row returned by the query.
                    if (result == null)
                    {
                        if (deliveryDto != null)
                        {
                            delivery = new Delivery
                            {
                                Id = deliveryDto.Id,
                                DriverId = deliveryDto.DriverId,
                                ConfirmationCode = deliveryDto.ConfirmationCode,
                                Status = deliveryDto.Status,
                                DeliveredAt = deliveryDto.DeliveredAt,
                                RouteJson = deliveryDto.Route,
                                PaymentAmount = deliveryDto.PaymentAmount,
                            };
                        }

                        result = new Order
                        {
                            Id = order.Id,
                            CustomerId = order.CustomerId,
                            FoodPlaceId = order.FoodPlaceId,
                            DeliveryAddressId = order.DeliveryAddressId,
                            ServiceFee = order.ServiceFee,
                            DeliveryFee = order.DeliveryFee,
                            Status = order.Status,
                            CreatedAt = order.CreatedAt,
                            Items = new List<OrderItem>(),
                            Delivery = delivery,
                            Payment = payment,
                        };
                    }
                    items.Add(item);

                    return null;
                },
                parameters,
                splitOn: "item_id, id, amount"
            );

            return new Order
            {
                Id = result!.Id,
                CustomerId = result.CustomerId,
                FoodPlaceId = result.FoodPlaceId,
                DeliveryAddressId = result.DeliveryAddressId,
                ServiceFee = result.ServiceFee,
                DeliveryFee = result.DeliveryFee,
                Status = result.Status,
                CreatedAt = result.CreatedAt,
                Items = items.AsReadOnly(),
                Delivery = result.Delivery,
                Payment = result.Payment,
            };
        }
        ;
    }

    public async Task<bool> UpdateOrderStatus(Order order)
    {
        var parameters = new { order.Id, order.Status };

        const string sql =
            @"
            UPDATE orders
            SET status = @Status
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            int rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        ;
    }

    public async Task<int> AddDelivery(int orderId, Delivery delivery)
    {
        var parameters = new
        {
            OrderId = orderId,
            delivery.ConfirmationCode,
            delivery.Status,
        };

        const string sql =
            @"
            INSERT INTO deliveries(order_id, confirmation_code, status)
            VALUES (@OrderId, @ConfirmationCode, @Status)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    // public async Task<Delivery?> GetDeliveryByOrderId(int orderId)
    // {
    //     var parameters = new { OrderId = orderId };

    //     const string sql =
    //         @"
    //             SELECT *
    //             FROM deliveries
    //             WHERE order_id = @OrderId
    //         ";
    //     using (var connection = new NpgsqlConnection(_connectionString))
    //     {
    //         return await connection.QuerySingleOrDefaultAsync<Delivery>(sql, parameters);
    //     }
    //     ;
    // }

    public async Task UpdateDelivery(int orderId, Delivery delivery)
    {
        var routeJson = delivery.RouteJson;

        var parameters = new
        {
            orderId,
            delivery.DriverId,
            delivery.Status,
            delivery.DeliveredAt,
            Route = routeJson,
            delivery.PaymentAmount,
        };

        const string sql =
            @"
                UPDATE deliveries
                SET status = @Status, driver_id = @DriverId, delivered_at = @DeliveredAt, route = @Route::jsonb, payment_amount = @PaymentAmount
                WHERE order_id = @orderId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task AddPayment(int orderId, Payment payment)
    {
        var parameters = new
        {
            orderId,
            payment.StripePaymentIntentId,
            payment.Amount,
            payment.Status,
        };

        const string sql =
            @"
            INSERT INTO payments(order_id, stripe_payment_intent_id, amount, status)
            VALUES (@orderId, @StripePaymentIntentId, @Amount, @Status)";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task<bool> UpdatePaymentStatus(int orderId, Payment payment)
    {
        var parameters = new { orderId, payment.Status };

        const string sql =
            @"
            UPDATE payments
            SET status = @Status
            WHERE order_id = @orderId";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            int rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
    }

    private class QueriedOrderDTO
    {
        public required int Id { get; init; }
        public required int CustomerId { get; init; }
        public required int FoodPlaceId { get; init; }
        public required int DeliveryAddressId { get; init; }
        public required int ServiceFee { get; init; }
        public required int DeliveryFee { get; set; }
        public required OrderStatuses Status { get; set; }
        public required DateTime CreatedAt { get; init; }
    }

    private class QueriedDeliveryDTO
    {
        public int Id { get; init; }
        public int DriverId { get; set; }
        public required string ConfirmationCode { get; init; }
        public required DeliveryStatuses Status { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string? Route { get; set; }
        public int? PaymentAmount { get; set; }
    }
}
