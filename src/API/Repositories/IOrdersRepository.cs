using Npgsql;

public interface IOrdersRepository
{
    Task<bool> CancelOrder(int id);
    Task<int> CreateOrder(CreateOrderDTO dto, NpgsqlTransaction? transaction = null);
    Task<Order?> GetOrderById(int id);
}
