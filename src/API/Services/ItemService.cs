public class ItemService : IItemService
{
    private readonly IItemsRepository _itemsRepo;

    public ItemService(IItemsRepository itemsRepository)
    {
        _itemsRepo = itemsRepository;
    }

    public async Task<int> CreateItem(ItemCreateRequest req)
    {
        return await _itemsRepo.CreateItem(req);
    }

    public async Task<ItemResponse?> GetItemAsync(int id)
    {
        var item = await _itemsRepo.GetItemById(id);
        if (item == null)
        {
            return null;
        }
        return MapEntityToResponse(item!);
    }

    public async Task<bool> UpdateItemAsync(ItemUpdateRequest req)
    {
        return await _itemsRepo.UpdateItem(req);
    }

    private ItemResponse MapEntityToResponse(Item item)
    {
        return new ItemResponse
        {
            Id = item!.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
        };
    }
}
