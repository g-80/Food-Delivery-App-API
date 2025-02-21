using System.ComponentModel.DataAnnotations;

public class QuoteTokenPayload
{
    [Required]
    public int CustomerId { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "Items cannot be empty")]
    public required List<RequestedItem> Items { get; set; }
    [Required]
    [Range(1, 100000, ErrorMessage = "Total price must be greater than 0")]
    public int TotalPrice { get; set; }
    [Required]
    public DateTime ExpiresAt { get; set; }
}