public class FoodPlacesService : IFoodPlacesService
{
    private readonly IFoodPlacesRepository _foodPlacesRepo;

    public FoodPlacesService(IFoodPlacesRepository foodPlacesRepository)
    {
        _foodPlacesRepo = foodPlacesRepository;
    }

    public async Task<int> CreateFoodPlaceAsync(FoodPlaceCreateRequest req)
    {
        return await _foodPlacesRepo.CreateFoodPlace(req);
    }

    public async Task<FoodPlace?> GetFoodPlaceAsync(int id)
    {
        return await _foodPlacesRepo.GetFoodPlace(id);
    }

    public async Task<IEnumerable<FoodPlace>> GetFoodPlacesWithinDistance(
        NearbyFoodPlacesRequest query
    )
    {
        return await _foodPlacesRepo.GetFoodPlacesWithinDistance(query);
    }

    public async Task<IEnumerable<FoodPlace>> SearchFoodPlacesWithinDistance(
        SearchFoodPlacesRequest query
    )
    {
        return await _foodPlacesRepo.SearchFoodPlacesWithinDistance(query);
    }
}
