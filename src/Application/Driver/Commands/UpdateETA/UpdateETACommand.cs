public class UpdateETACommand
{
    public required int DriverId { get; init; }
    public required int DeliveryId { get; init; }
    public required TimeSpan NewETA { get; init; }
}
