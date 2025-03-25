public class EmptyCartException : Exception
{
    public EmptyCartException(string message = "No cart items found matching cart id") : base(message) { }
}