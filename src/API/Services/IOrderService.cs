public interface IOrderService
{
    Task<bool> CancelOrderAsync(int orderId);
    Task<int> CreateOrderAsync(int customerId);
    Task<Order?> GetOrderByIdAsync(int orderId);
}
