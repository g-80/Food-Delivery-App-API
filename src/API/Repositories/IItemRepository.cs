public interface IItemsRepository
{
    Task<int> CreateItem(ItemCreateRequest itemReq);
    Task<Item?> GetItemById(int id);
    Task<bool> UpdateItem(ItemUpdateRequest itemReq);
}
