public class CartItem
{
    public required int CartId { get; init; }
    public required int ItemId { get; init; }
    private int _quantity;
    public required int Quantity
    {
        get => _quantity;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Quantity cannot be less than one");
            }
            _quantity = value;
        }
    }
    public required int UnitPrice { get; init; }
    public int Subtotal => UnitPrice * Quantity;
}
