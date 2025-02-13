public class QuoteResponse
{
    public int QuoteId { get; set; }
    public string QuoteToken { get; set; } = string.Empty;
    public QuoteTokenPayload QuoteTokenPayload { get; set; }
}