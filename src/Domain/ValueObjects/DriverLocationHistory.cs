public class DriverLocationHistory
{
    public required int Id { get; init; }
    public required int DriverId { get; init; }
    public required Location Location { get; init; }
    public required double Accuracy { get; init; }
    public double? Speed { get; init; }
    public double? Heading { get; init; }
    public required DateTime Timestamp { get; init; }
    public int? DeliveryId { get; init; }
}