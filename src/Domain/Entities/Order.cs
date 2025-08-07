public class Order
{
    public int Id { get; init; }
    public required int CustomerId { get; init; }
    public required int FoodPlaceId { get; init; }
    public required int DeliveryAddressId { get; init; }
    public IReadOnlyList<OrderItem>? Items { get; init; }
    public required int Subtotal { get; init; }
    public required int ServiceFee { get; init; }
    public required int DeliveryFee { get; set; }
    public required int Total { get; init; }
    public Delivery? Delivery { get; set; }
    public required OrderStatuses Status { get; set; }
    public required DateTime CreatedAt { get; init; }

    public void CreateDelivery()
    {
        Delivery = new Delivery
        {
            AddressId = DeliveryAddressId,
            ConfirmationCode = new Random().Next(1000, 9999).ToString(),
            Status = DeliveryStatuses.assigningDriver,
        };
    }
}
