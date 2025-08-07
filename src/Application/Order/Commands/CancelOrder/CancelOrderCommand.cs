using System.ComponentModel.DataAnnotations;

public class CancelOrderCommand
{
    [Required]
    public required string Reason { get; init; }
}
