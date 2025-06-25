using System.ComponentModel.DataAnnotations;

public class OrderConfirmationRequest
{
    [Required]
    public required bool Confirmed { get; init; }
}
