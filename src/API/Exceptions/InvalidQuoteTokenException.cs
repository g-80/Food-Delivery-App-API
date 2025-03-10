public class InvalidQuoteTokenException : Exception
{
    public InvalidQuoteTokenException(string message = "Quote token verification failed. The quote is invalid or corrupted.") : base(message) { }
}