public class QuoteItem
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalPrice { get; set; }
}