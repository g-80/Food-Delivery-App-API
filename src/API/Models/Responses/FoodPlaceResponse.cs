public class FoodPlaceResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string Category { get; init; }
    public required double? Distance { get; init; }
}
