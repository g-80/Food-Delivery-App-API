using System.ComponentModel.DataAnnotations;

public class AddItemToCartRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public required RequestedItem Item { get; set; }
}