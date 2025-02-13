public class Quote
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Price { get; set; }
    public bool IsUsed { get; set; }
}