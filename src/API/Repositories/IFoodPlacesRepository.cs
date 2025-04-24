public interface IFoodPlacesRepository
{
    Task<int> CreateFoodPlace(FoodPlaceCreateRequest request);
    Task<FoodPlace?> GetFoodPlace(int id);
    Task<IEnumerable<FoodPlace>> GetFoodPlacesWithinDistance(FoodPlacesNearbyRequest query);
    Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(FoodPlacesSearchRequest query);
}
