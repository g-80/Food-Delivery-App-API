using System.Collections.Concurrent;

public class DeliveriesAssignments : IDeliveriesAssignments
{
    private readonly ConcurrentDictionary<int, DeliveryAssignmentJob> _activeAssignments = new();

    public DeliveryAssignmentJob? GetAssignmentJob(int orderId)
    {
        _activeAssignments.TryGetValue(orderId, out var job);
        return job;
    }

    public void RemoveAssignmentJob(int orderId)
    {
        _activeAssignments.TryRemove(orderId, out _);
    }

    public DeliveryAssignmentJob CreateAssignmentJob(int orderId)
    {
        var job = new DeliveryAssignmentJob { OrderId = orderId };
        _activeAssignments.TryAdd(orderId, job);
        return job;
    }
}
