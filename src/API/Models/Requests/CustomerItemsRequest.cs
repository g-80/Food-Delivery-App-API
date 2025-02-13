using System.ComponentModel.DataAnnotations;

public class CustomerItemsRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Items cannot be empty")]
    public List<ItemRequest> Items { get; set; } = new List<ItemRequest>();
}