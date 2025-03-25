public class CartNotFoundException : Exception
{
    public CartNotFoundException(string message = "Cart not found") : base(message) { }
}