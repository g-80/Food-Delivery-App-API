using System.ComponentModel.DataAnnotations;

public class UpdateItemQuantityCommand
{
    [Required]
    public required int ItemId { get; init; }

    [Required]
    [Range(0, 20)]
    public required int Quantity { get; init; }
}
