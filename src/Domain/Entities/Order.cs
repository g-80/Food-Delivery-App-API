public class Order
{
    public int Id { get; set; }
    public required int CustomerId { get; init; }
    public required int FoodPlaceId { get; init; }
    public required int DeliveryAddressId { get; init; }
    public required IReadOnlyList<OrderItem> Items { get; init; }
    public int Subtotal => Items.Sum(item => item.UnitPrice * item.Quantity);
    public required int ServiceFee { get; init; }
    public required int DeliveryFee { get; set; }
    public int Total => Subtotal + ServiceFee + DeliveryFee;
    public Delivery? Delivery { get; set; }
    public Payment? Payment { get; set; }
    public required OrderStatuses Status { get; set; }
    public required DateTime CreatedAt { get; init; }

    public void CreateDelivery()
    {
        Delivery = new Delivery
        {
            ConfirmationCode = new Random().Next(1000, 9999).ToString(),
            Status = DeliveryStatuses.assigningDriver,
        };
    }
}
