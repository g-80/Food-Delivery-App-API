public class CreateCartItemDTO
{
    public required int CartId { get; init; }
    public required int ItemId { get; init; }
    public required int Quantity { get; init; }
    public required int UnitPrice { get; init; }
    public required int Subtotal { get; init; }
}
