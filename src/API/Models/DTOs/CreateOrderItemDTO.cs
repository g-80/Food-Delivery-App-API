public class CreateOrderItemDTO
{
    public required RequestedItem RequestedItem { get; set; }
    public int OrderId { get; set; }
    public int Subtotal { get; set; }
}