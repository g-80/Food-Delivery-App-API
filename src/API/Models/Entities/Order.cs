public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalPrice { get; set; }
    public bool IsCancelled { get; set; }
}