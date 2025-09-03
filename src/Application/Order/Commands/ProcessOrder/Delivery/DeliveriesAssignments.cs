using System.Collections.Concurrent;

public class DeliveriesAssignments : IDeliveriesAssignments
{
    private readonly ConcurrentDictionary<int, DeliveryAssignmentJob> _activeAssignments = new();

    public DeliveryAssignmentJob GetAssignmentJob(int orderId)
    {
        if (!_activeAssignments.TryGetValue(orderId, out var job))
        {
            throw new InvalidOperationException(
                $"Delivery assignment job for order {orderId} not found"
            );
        }
        return job;
    }

    public void RemoveAssignmentJob(int orderId)
    {
        _activeAssignments.TryRemove(orderId, out _);
    }

    public DeliveryAssignmentJob CreateAssignmentJob(int orderId)
    {
        var job = new DeliveryAssignmentJob
        {
            OrderId = orderId,
            CurrentAttempt = 0,
            AssignedDriverId = 0,
            PendingOffers = new ConcurrentDictionary<int, CancellationTokenSource>(),
        };
        _activeAssignments.TryAdd(orderId, job);
        return job;
    }
}
