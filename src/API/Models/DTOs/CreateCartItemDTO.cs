public class CreateCartItemDTO
{
    public required RequestedItem RequestedItem { get; set; }
    public int CartId { get; set; }
    public int UnitPrice { get; set; }
    public int Subtotal { get; set; }
}