public class AddDriverLocationHistoryCommand
{
    public required int DriverId { get; init; }
    public required Location Location { get; init; }
    public required double Accuracy { get; init; }
    public required double Speed { get; init; }
    public required double Heading { get; init; }
    public required int DeliveryId { get; init; }
}
