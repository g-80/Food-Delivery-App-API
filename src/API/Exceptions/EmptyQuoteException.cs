public class EmptyQuoteException : Exception
{
    public EmptyQuoteException(string message = "No quote items found matching quote") : base(message) { }
}