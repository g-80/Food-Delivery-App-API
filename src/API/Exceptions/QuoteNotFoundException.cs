public class QuoteNotFoundException : Exception
{
    public QuoteNotFoundException(string message = "Quote not found") : base(message) { }
}