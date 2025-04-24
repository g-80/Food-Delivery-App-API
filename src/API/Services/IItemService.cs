public interface IItemService
{
    Task<int> CreateItem(ItemCreateRequest req);
    Task<ItemResponse?> GetItemAsync(int id);
    Task<bool> UpdateItemAsync(ItemUpdateRequest req);
}
