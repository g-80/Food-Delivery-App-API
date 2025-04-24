using System.ComponentModel.DataAnnotations;

public class CartUpdateItemQuantityRequest
{
    [Required]
    [Range(1, 20)]
    public int Quantity { get; set; }
}
