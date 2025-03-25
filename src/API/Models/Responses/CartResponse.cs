public class CartResponse
{
    public required IEnumerable<CartItemResponse> CartItems { get; set; }
    public int Subtotal { get; set; }
    public int Fees { get; set; }
    public int DeliveryFee { get; set; }
    public int Total { get; set; }
}