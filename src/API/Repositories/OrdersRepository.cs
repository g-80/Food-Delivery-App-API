using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class OrdersRepository : BaseRepository, IOrdersRepository
{
    public OrdersRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<Order?> GetOrderById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT *
            FROM orders
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<Order>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateOrder(CreateOrderDTO dto)
    {
        var parameters = new
        {
            dto.CustomerId,
            dto.TotalPrice,
            dto.FoodPlaceId,
            dto.DeliveryAddressId,
            dto.Status,
        };
        const string sql =
            @"
            INSERT INTO orders(customer_id, food_place_id, delivery_address_id, total_price, status)
            VALUES
            (@CustomerId, @FoodPlaceId, @DeliveryAddressId, @TotalPrice, @Status)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<bool> UpdateOrderStatus(int id, OrderStatuses newStatus)
    {
        var parameters = new { Id = id, Status = newStatus };

        const string sql =
            @"
            UPDATE orders
            SET status = @Status
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            int rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        ;
    }

    public async Task<OrderConfirmationDTO> GetOrderConfirmationDTO(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT o.id AS order_id, CONCAT(u.first_name, ' ', u.surname) AS customer_name, fi.name AS item_name, oi.quantity
            FROM orders o
            INNER JOIN users u ON o.customer_id = u.id
            INNER JOIN order_items oi ON o.id = oi.order_id
            INNER JOIN food_places_items fi ON oi.item_id = fi.id
            WHERE o.id = @Id
            ORDER BY fi.name
            ";

        OrderConfirmationDTO? result = null;
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            // <type to map the first part to, type to map the second part to, return type>
            var _ = await connection.QueryAsync<
                OrderConfirmationDTO,
                OrderConfirmationItemDTO,
                OrderConfirmationDTO?
            >(
                sql,
                (order, item) =>
                {
                    // This will run for each row returned by the query.
                    if (result == null)
                    {
                        result = new OrderConfirmationDTO
                        {
                            OrderId = order.OrderId,
                            CustomerName = order.CustomerName,
                            OrderItems = new List<OrderConfirmationItemDTO>(),
                        };
                    }

                    result.OrderItems.Add(item);

                    return null;
                },
                parameters,
                splitOn: "item_name"
            );

            return result!;
        }
    }

    public async Task<int> GetFoodPlaceUserIdAsync(int orderId)
    {
        var parameters = new { Id = orderId };
        const string sql =
            @"
            SELECT fp.user_id
            FROM orders o
            INNER JOIN food_places fp ON o.food_place_id = fp.id
            WHERE o.id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<int>(sql, parameters);
        }
    }
}
