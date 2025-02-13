using System.ComponentModel.DataAnnotations;

public class CreateQuoteRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Quote items cannot be empty")]
    public List<ItemRequest> Items { get; set; } = new List<ItemRequest>();
}