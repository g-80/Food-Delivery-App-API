using System.Collections.Concurrent;

public class DeliveriesAssignments
{
    private readonly ConcurrentDictionary<int, DeliveryAssignmentJob> _activeAssignments = new();

    public DeliveryAssignmentJob GetOrCreateAssignmentJob(int orderId)
    {
        return _activeAssignments.GetOrAdd(orderId, CreateNewAssignmentJob);
    }

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

    private DeliveryAssignmentJob CreateNewAssignmentJob(int orderId)
    {
        return new DeliveryAssignmentJob
        {
            OrderId = orderId,
            CurrentAttempt = 0,
            AssignedDriverId = 0,
            PendingOffers = new ConcurrentDictionary<int, CancellationTokenSource>(),
        };
    }
}
