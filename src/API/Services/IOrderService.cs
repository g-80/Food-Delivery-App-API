public interface IOrderService
{
    Task<bool> CancelOrderAsync(int orderId);
    Task<int> CreateOrderAsync(int customerId);
    Task<OrderResponse?> GetOrderByIdAsync(int orderId);
}
