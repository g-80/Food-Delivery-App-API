using System.ComponentModel.DataAnnotations;

public class CreateQuoteRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Quote items cannot be empty")]
    public required List<RequestedItem> Items { get; set; }
}