using Npgsql;

public interface IOrdersItemsRepository
{
    Task<int> CreateOrderItem(CreateOrderItemDTO dto, NpgsqlTransaction? transaction = null);
    Task<OrderItem?> GetOrderItemById(int id);
    Task<IEnumerable<OrderItem>> GetOrderItemsByOrderId(int orderId);
}
