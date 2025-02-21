public class QuoteResponse
{
    public int QuoteId { get; set; }
    public required string QuoteToken { get; set; }
    public required QuoteTokenPayload QuoteTokenPayload { get; set; }
}