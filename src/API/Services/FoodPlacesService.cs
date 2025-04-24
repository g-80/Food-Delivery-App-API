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

    public async Task<FoodPlaceResponse?> GetFoodPlaceAsync(int id)
    {
        var foodPlace = await _foodPlacesRepo.GetFoodPlace(id);
        if (foodPlace == null)
        {
            return null;
        }
        return MapEntityToResponse(foodPlace!);
    }

    public async Task<IEnumerable<FoodPlaceResponse>> GetFoodPlacesWithinDistance(
        FoodPlacesNearbyRequest query
    )
    {
        var foodPlaces = await _foodPlacesRepo.GetFoodPlacesWithinDistance(query);
        return foodPlaces.Select(MapEntityToResponse);
    }

    public async Task<IEnumerable<FoodPlaceResponse>> SearchFoodPlacesWithinDistance(
        FoodPlacesSearchRequest query
    )
    {
        var foodPlaces = await _foodPlacesRepo.SearchFoodPlacesWithinDistance(query);
        return foodPlaces.Select(MapEntityToResponse);
    }

    private FoodPlaceResponse MapEntityToResponse(FoodPlace foodPlace)
    {
        return new FoodPlaceResponse
        {
            Id = foodPlace!.Id,
            Name = foodPlace.Name,
            Description = foodPlace.Description,
            Category = foodPlace.Category,
            Distance = foodPlace.Distance,
        };
    }
}
