public interface IFoodPlacesService
{
    Task<int> CreateFoodPlaceAsync(FoodPlaceCreateRequest req);
    Task<FoodPlace?> GetFoodPlaceAsync(int id);
    Task<IEnumerable<FoodPlace>> GetFoodPlacesWithinDistance(NearbyFoodPlacesRequest query);
    Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(SearchFoodPlacesRequest query);
}
