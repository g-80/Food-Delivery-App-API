public interface IOrderService
{
    Task<bool> CancelOrderAsync(int orderId);
    Task<List<int>> CreateOrderAsync(int customerId, OrderCreateRequest request);
    Task<int> GetFoodPlaceUserIdAsync(int orderId);
    Task<OrderConfirmationDTO> GetOrderConfirmationDTOAsync(int orderId);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<OrderResponse?> GetOrderResponseByIdAsync(int orderId);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatuses status);
}
