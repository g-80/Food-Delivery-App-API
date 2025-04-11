public interface IItemsRepository
{
    Task<int> CreateItem(CreateItemRequest itemReq);
    Task<Item?> GetItemById(int id);
    Task<bool> UpdateItem(UpdateItemRequest itemReq);
}