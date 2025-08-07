public class FoodPlace
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string Category { get; init; }
    public required int AddressId { get; init; }
    public required Location Location { get; init; }

    private readonly List<FoodPlaceItem> _items = new();
    public IEnumerable<FoodPlaceItem> Items
    {
        get => _items.AsReadOnly();
        init => _items = value.ToList();
    }

    public void AddItem(FoodPlaceItem item)
    {
        _items!.Add(item);
    }

    public void UpdateItem(FoodPlaceItem item)
    {
        var existingItem = _items!.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.Name = item.Name;
            existingItem.Price = item.Price;
            existingItem.Description = item.Description;
        }
    }
}
