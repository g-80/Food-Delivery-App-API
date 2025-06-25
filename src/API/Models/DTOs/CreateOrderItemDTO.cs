public class CreateOrderItemDTO
{
    public required int OrderId { get; init; }
    public required int ItemId { get; init; }
    public required int Quantity { get; init; }
    public required int UnitPrice { get; init; }
    public required int Subtotal { get; init; }
}
