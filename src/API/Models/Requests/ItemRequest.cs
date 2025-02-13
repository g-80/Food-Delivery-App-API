using System.ComponentModel.DataAnnotations;

public class ItemRequest
{
    [Required]
    public int ItemId { get; set; }
    [Required]
    [Range(1, 20)]
    public int Quantity { get; set; }
}