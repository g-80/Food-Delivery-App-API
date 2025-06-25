public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int FoodPlaceId { get; set; }
    public int DeliveryAddressId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalPrice { get; set; }
    public OrderStatuses Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}
