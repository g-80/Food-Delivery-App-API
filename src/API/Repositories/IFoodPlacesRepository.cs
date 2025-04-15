public interface IFoodPlacesRepository
{
    Task<int> CreateFoodPlace(FoodPlaceCreateRequest request);
    Task<FoodPlace?> GetFoodPlace(int id);
    Task<IEnumerable<FoodPlace>> GetFoodPlacesWithinDistance(NearbyFoodPlacesRequest query);
    Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(SearchFoodPlacesRequest query);
}
