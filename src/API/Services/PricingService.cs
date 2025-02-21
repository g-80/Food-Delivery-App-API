public class PricingService
{
    private readonly ItemsRepository _itemsRepository;

    public PricingService(ItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }

    public async Task<(List<int>, int)> CalculatePriceAsync(List<RequestedItem> items)
    {
        int totalPrice = 0;
        List<int> prices = new();

        foreach (var itemReq in items)
        {
            Item item = await _itemsRepository.GetItemById(itemReq.ItemId);
            if (item == null) throw new Exception($"Item with ID: {itemReq.ItemId} not found");

            int price = item.Price * itemReq.Quantity;
            prices.Add(price);
            totalPrice += price;
        }
        return (prices, totalPrice);
    }
}