public class CreateDeliveryDTO
{
    public required int OrderId { get; init; }
    public required int AddressId { get; init; }
    public required string ConfirmationCode { get; init; }
    public required DeliveryStatuses Status { get; init; }
}
