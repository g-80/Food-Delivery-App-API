public class Delivery
{
    public required int Id { get; init; }
    public required int OrderId { get; init; }
    public required int AddressId { get; init; }
    public required int DriverId { get; init; }
    public required bool IsDelivered { get; init; }
    public required DateTime DeliveredAt { get; init; }
    public required DateTime CreatedAt { get; init; }
}
