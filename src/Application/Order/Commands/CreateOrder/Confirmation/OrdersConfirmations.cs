public class OrdersConfirmations
{
    private readonly Dictionary<int, OrderConfirmation> _activeConfirmations = new();

    public void AddOrderConfirmation(int orderId, OrderConfirmation cts)
    {
        if (_activeConfirmations.ContainsKey(orderId))
        {
            throw new Exception($"CancellationTokenSource for order {orderId} already exists");
        }
        _activeConfirmations[orderId] = cts;
    }

    public OrderConfirmation GetOrderConfirmation(int orderId)
    {
        if (!_activeConfirmations.TryGetValue(orderId, out var cts))
        {
            throw new Exception($"CancellationTokenSource for order {orderId} not found");
        }
        return cts;
    }

    public void RemoveOrderConfirmation(int orderId)
    {
        _activeConfirmations.Remove(orderId, out _);
    }
}
