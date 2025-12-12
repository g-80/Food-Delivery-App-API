public class OrderItem
{
    public required int ItemId { get; init; }
    public required int Quantity { get; init; }
    public required int UnitPrice { get; init; }
    public int Subtotal => UnitPrice * Quantity;
}
