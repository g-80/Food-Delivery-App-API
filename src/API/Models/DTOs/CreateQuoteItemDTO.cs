public class CreateQuoteItemDTO
{
    public required RequestedItem RequestedItem { get; set; }
    public int QuoteId { get; set; }
    public int TotalPrice { get; set; }
}