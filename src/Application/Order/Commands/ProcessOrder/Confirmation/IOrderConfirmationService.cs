public interface IOrderConfirmationService
{
    Task<bool> RequestOrderConfirmation(Order order);
    void ConfirmOrder(int orderId);
    void RejectOrder(int orderId);
}
