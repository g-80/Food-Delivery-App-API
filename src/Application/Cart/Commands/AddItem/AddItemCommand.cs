using System.ComponentModel.DataAnnotations;

public class AddItemCommand
{
    [Required]
    public int ItemId { get; init; }

    [Required]
    [Range(1, 20)]
    public int Quantity { get; init; }
}
