public class CartPricing
{
    public required int CartId { get; init; }
    public required int Subtotal { get; set; }
    public required int ServiceFee { get; set; }
    public required int DeliveryFee { get; set; }
    public required int Total { get; set; }

    public void UpdatePricing(int subtotal, int serviceFee, int deliveryFee)
    {
        Subtotal = subtotal;
        ServiceFee = serviceFee;
        DeliveryFee = deliveryFee;
        Total = CalculateTotal();
    }

    private int CalculateTotal()
    {
        return Subtotal + ServiceFee + DeliveryFee;
    }
}
