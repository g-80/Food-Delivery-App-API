public interface IFoodPlaceRepository
{
    Task<int> AddFoodPlace(FoodPlace foodPlace, int userId);
    Task<FoodPlace?> GetFoodPlaceById(int id);
    Task<FoodPlace?> GetFoodPlaceByItemId(int itemId);
    Task<FoodPlace?> GetFoodPlaceByUserId(int userId);
    Task<int?> GetFoodPlaceUserId(int foodPlaceId);
    Task<IEnumerable<FoodPlace>> GetNearbyFoodPlaces(
        GetNearbyFoodPlacesQuery query,
        int distanceMeters = 3000
    );
    Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(
        SearchFoodPlacesQuery query,
        int distanceMeters = 3000
    );
    Task UpdateFoodPlace(FoodPlace foodPlace);
    Task AddFoodPlaceItem(int foodPlaceId, FoodPlaceItem item);
    Task UpdateFoodPlaceItem(FoodPlaceItem item);
}
