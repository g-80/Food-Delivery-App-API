using System.ComponentModel.DataAnnotations;

public class UpdateItemRequest
{
    [Required]
    public int Id { get; set; }
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Name { get; set; }
    [MaxLength(100)]
    public string? Description { get; set; }
    [Required]
    [Range(1, 10000, ErrorMessage = "Invalid price")]
    public int Price { get; set; }
    [Required]
    public bool IsAvailable { get; set; }
}