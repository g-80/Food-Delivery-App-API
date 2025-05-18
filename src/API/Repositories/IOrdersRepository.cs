public interface IOrdersRepository
{
    Task<bool> CancelOrder(int id);
    Task<int> CreateOrder(CreateOrderDTO dto);
    Task<Order?> GetOrderById(int id);
}
