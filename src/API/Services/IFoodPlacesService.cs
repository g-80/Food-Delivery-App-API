public interface IFoodPlacesService
{
    Task<int> CreateFoodPlaceAsync(FoodPlaceCreateRequest req);
    Task<FoodPlaceResponse?> GetFoodPlaceAsync(int id);
    Task<IEnumerable<FoodPlaceResponse>> GetFoodPlacesWithinDistance(FoodPlacesNearbyRequest query);
    Task<IEnumerable<FoodPlaceResponse>> SearchFoodPlacesWithinDistance(
        FoodPlacesSearchRequest query
    );
}
