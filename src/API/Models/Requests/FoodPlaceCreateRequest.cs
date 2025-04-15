using System.ComponentModel.DataAnnotations;

public class FoodPlaceCreateRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Category { get; set; }

    [Required]
    [Range(49.0, 59.0, ErrorMessage = "Invalid location")]
    public double Latitude { get; set; }

    [Required]
    [Range(-8.0, 2.0, ErrorMessage = "Invalid location")]
    public double Longitude { get; set; }
}
