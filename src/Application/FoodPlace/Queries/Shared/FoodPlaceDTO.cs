public class FoodPlaceDTO
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string Category { get; init; }
    public IEnumerable<GetFoodPlaceItemDTO>? Items { get; init; }
}
