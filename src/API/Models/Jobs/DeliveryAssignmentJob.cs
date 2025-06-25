using System.Collections.Concurrent;

public class DeliveryAssignmentJob
{
    public int OrderId { get; set; }
    public int CurrentAttempt { get; set; } = 0;
    public int AssignedDriverId { get; set; } = 0;
    public ConcurrentDictionary<int, CancellationTokenSource> PendingOffers { get; set; } = new();
}
