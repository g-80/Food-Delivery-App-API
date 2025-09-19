public class Cart
{
    public int Id { get; init; }
    public required int CustomerId { get; init; }
    public required DateTime ExpiresAt { get; set; }

    private List<CartItem> _items = new();
    public required IEnumerable<CartItem> Items
    {
        get => _items.AsReadOnly();
        init => _items = value.ToList();
    }
    public int FoodPlaceId { get; set; }
    public required CartPricing Pricing { get; init; }
    public bool IsModified { get; set; } = false;
    private readonly TimeSpan _cartExpirationTime = TimeSpan.FromMinutes(5);

    public void AddItem(CartItem item, int fromFoodPlace)
    {
        if (_items.Any() && fromFoodPlace != FoodPlaceId)
        {
            _items.Clear();
            FoodPlaceId = fromFoodPlace;
        }
        var existingItem = _items.FirstOrDefault(i => i.ItemId == item.ItemId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            if (FoodPlaceId == 0)
            {
                FoodPlaceId = fromFoodPlace;
            }
            _items.Add(item);
        }
        RecalculatePricing();
        UpdateExpiry();
    }

    public void RemoveItem(int itemId)
    {
        var item = _items.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            _items.Remove(item);
            if (_items.Count == 0)
            {
                FoodPlaceId = 0;
            }
            IsModified = true;
            RecalculatePricing();
            UpdateExpiry();
        }
        else
        {
            throw new InvalidOperationException($"Item with ID: {itemId} not found in cart {Id}");
        }
    }

    public void UpdateItemQuantity(int itemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            if (item.Quantity == quantity)
            {
                return;
            }
            item.Quantity = quantity;
            IsModified = true;
            RecalculatePricing();
            UpdateExpiry();
        }
        else
        {
            throw new InvalidOperationException($"Item with ID: {itemId} not found in cart {Id}");
        }
    }

    public void ClearCart()
    {
        _items.Clear();
        FoodPlaceId = 0;
        IsModified = false;
        RecalculatePricing();
        UpdateExpiry();
    }

    private void RecalculatePricing()
    {
        int subtotal = _items.Sum(item => item.Subtotal);
        if (subtotal == 0)
        {
            Pricing.UpdatePricing(0, 0, 0);
            return;
        }
        int serviceFee = 0.1 * subtotal < 50 ? 50 : Convert.ToInt32(0.1 * subtotal);
        int deliveryFee = 250;

        Pricing.UpdatePricing(subtotal, serviceFee, deliveryFee);
    }

    private void UpdateExpiry()
    {
        ExpiresAt = DateTime.UtcNow.Add(_cartExpirationTime);
    }
}
