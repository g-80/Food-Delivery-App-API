using System.ComponentModel.DataAnnotations;

public class CreateItemRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Description { get; set; } = string.Empty;
    [Required]
    public int FoodPlaceId { get; set; }
    [Required]
    [Range(1, 10000, ErrorMessage = "Invalid price")]
    public int Price { get; set; }
    [Required]
    public bool isAvailable { get; set; }
}