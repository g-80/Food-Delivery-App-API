public class FoodPlace
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}