using System.ComponentModel.DataAnnotations;

public class OrderRequest
{
    [Required]
    public int QuoteId { get; set; }
    [Required]
    public string QuoteToken { get; set; } = string.Empty;
    [Required]
    public QuoteTokenPayload QuoteTokenPayload { get; set; }
}