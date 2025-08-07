public interface IOrderRepository
{
    Task<int> AddOrder(Order order);
    Task<int> AddDelivery(int orderId, Delivery delivery);
    Task<IEnumerable<Order>> GetAllOrdersByCustomerId(int customerId);
    Task<Order?> GetOrderById(int id);

    // Task<Delivery?> GetDeliveryByOrderId(int orderId);
    Task<bool> UpdateOrderStatus(Order order);
    Task UpdateDelivery(int orderId, Delivery delivery);
}
