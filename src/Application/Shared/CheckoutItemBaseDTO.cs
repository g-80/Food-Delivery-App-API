public abstract class CheckoutItemBaseDTO
{
    public required int ItemId { get; init; }
    public required string ItemName { get; init; }
    public required int Quantity { get; init; }
    public required int UnitPrice { get; init; }
    public required int Subtotal { get; init; }
}
