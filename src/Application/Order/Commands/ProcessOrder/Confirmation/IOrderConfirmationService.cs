public interface IOrderConfirmationService
{
    Task<bool> RequestOrderConfirmation(Order order);
    Task<bool> ConfirmOrder(int orderId, int userId);
    Task<bool> RejectOrder(int orderId, int userId);
}
