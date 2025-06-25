public interface IOrdersRepository
{
    Task<int> CreateOrder(CreateOrderDTO dto);
    Task<int> GetFoodPlaceUserIdAsync(int orderId);
    Task<Order?> GetOrderById(int id);
    Task<OrderConfirmationDTO> GetOrderConfirmationDTO(int id);
    Task<bool> UpdateOrderStatus(int id, OrderStatuses status);
}
