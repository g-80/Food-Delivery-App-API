using System.Collections.Concurrent;

public class OrdersAssignments
{
    private readonly ConcurrentDictionary<int, OrderAssignmentJob> _activeAssignments = new();

    public OrderAssignmentJob GetOrCreateAssignmentJob(int orderId)
    {
        return _activeAssignments.GetOrAdd(orderId, CreateNewAssignmentJob);
    }

    public OrderAssignmentJob GetAssignmentJob(int orderId)
    {
        if (!_activeAssignments.TryGetValue(orderId, out var job))
        {
            throw new Exception($"Order assignment job for order {orderId} not found");
        }
        return job;
    }

    public void RemoveAssignmentJob(int orderId)
    {
        _activeAssignments.TryRemove(orderId, out _);
    }

    private OrderAssignmentJob CreateNewAssignmentJob(int orderId)
    {
        return new OrderAssignmentJob
        {
            OrderId = orderId,
            CurrentAttempt = 0,
            AssignedDriverId = 0,
            PendingOffers = new ConcurrentDictionary<int, CancellationTokenSource>(),
        };
    }
}
