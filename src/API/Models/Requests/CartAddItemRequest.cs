using System.ComponentModel.DataAnnotations;

public class CartAddItemRequest
{
    [Required]
    public required RequestedItem Item { get; set; }
}
