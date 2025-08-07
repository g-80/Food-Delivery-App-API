public abstract class CheckoutDTO<TItem>
    where TItem : CheckoutItemBaseDTO
{
    public required int FoodPlaceId { get; init; }
    public required string FoodPlaceName { get; init; }
    public required IEnumerable<TItem> Items { get; init; }
    public required int Subtotal { get; init; }
    public required int Fees { get; init; }
    public required int DeliveryFee { get; init; }
    public required int Total { get; init; }
}
