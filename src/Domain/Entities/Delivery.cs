public class Delivery
{
    public int Id { get; init; }
    public int DriverId { get; set; }
    public required string ConfirmationCode { get; init; }
    public required DeliveryStatuses Status { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? RouteJson { get; set; }
    public int? PaymentAmount { get; set; }
}
