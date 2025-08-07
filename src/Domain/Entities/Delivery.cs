public class Delivery
{
    public int Id { get; init; }
    public required int AddressId { get; init; }
    public int DriverId { get; set; }
    public required string ConfirmationCode { get; init; }
    public required DeliveryStatuses Status { get; set; }
    public DateTime DeliveredAt { get; set; }
}
