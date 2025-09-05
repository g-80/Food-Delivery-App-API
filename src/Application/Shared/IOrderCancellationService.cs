public interface IOrderCancellationService
{
    Task<bool> CancelOrder(Order order, string? reason = null);
}