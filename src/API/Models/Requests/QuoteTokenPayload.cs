public class QuoteTokenPayload
{
    public int UserId { get; set; }
    public List<ItemRequest> Items { get; set; }
    public int TotalPrice { get; set; }
    public DateTime ExpiresAt { get; set; }
}