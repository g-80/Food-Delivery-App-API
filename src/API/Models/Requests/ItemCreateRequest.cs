using System.ComponentModel.DataAnnotations;

public class ItemCreateRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    [Required]
    public required int FoodPlaceId { get; set; }

    [Required]
    [Range(1, 10000, ErrorMessage = "Invalid price")]
    public required int Price { get; set; }

    [Required]
    public required bool IsAvailable { get; set; }
}
