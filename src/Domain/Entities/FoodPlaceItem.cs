public class FoodPlaceItem
{
    public int Id { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required int Price { get; set; }
    public required bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; init; }
}
