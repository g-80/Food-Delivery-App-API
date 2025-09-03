public interface IOrdersConfirmations
{
    void AddOrderConfirmation(int orderId, OrderConfirmation cts);
    OrderConfirmation GetOrderConfirmation(int orderId);
    void RemoveOrderConfirmation(int orderId);
}
