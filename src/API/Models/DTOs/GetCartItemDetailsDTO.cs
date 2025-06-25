public class GetCartItemDetailsDTO
{
    public required int Id { get; init; }
    public required int FoodPlaceId { get; init; }
    public required int ItemId { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
    public required int Quantity { get; init; }
    public required int UnitPrice { get; init; }
    public required int Subtotal { get; init; }
}
