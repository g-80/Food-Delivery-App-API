public class DeliveryAssignmentJob
{
    public required int OrderId { get; init; }
    public int CurrentAttempt { get; set; } = 0;
    public int AssignedDriverId { get; set; } = 0;
    public int OfferedDriverId { get; set; } = 0;
    public CancellationTokenSource? Cts { get; set; }
    public string Route { get; set; } = string.Empty;
    public int Payment { get; set; } = 0;
}
