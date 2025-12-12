public class CartPricing
{
    public required int CartId { get; init; }
    public required int Subtotal { get; set; }
    public required int ServiceFee { get; set; }
    public required int DeliveryFee { get; set; }
    public int Total => Subtotal + ServiceFee + DeliveryFee;

    public void UpdatePricing(int subtotal, int serviceFee, int deliveryFee)
    {
        Subtotal = subtotal;
        ServiceFee = serviceFee;
        DeliveryFee = deliveryFee;
    }
}
