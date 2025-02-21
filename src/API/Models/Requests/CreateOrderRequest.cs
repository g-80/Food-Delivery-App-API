using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public int QuoteId { get; set; }
    [Required]
    public required string QuoteToken { get; set; }
    [Required]
    public required QuoteTokenPayload QuoteTokenPayload { get; set; }
}