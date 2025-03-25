using System.ComponentModel.DataAnnotations;

public class UpdateCartItemQuantityRequest
{
    [Required]
    [Range(1, 20)]
    public int Quantity { get; set; }
}
