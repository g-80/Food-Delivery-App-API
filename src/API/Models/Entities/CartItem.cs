public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UnitPrice { get; set; }
    public int Subtotal { get; set; }
}