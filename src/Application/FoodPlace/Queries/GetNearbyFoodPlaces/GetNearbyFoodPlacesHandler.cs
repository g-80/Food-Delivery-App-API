public class GetNearbyFoodPlacesHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public GetNearbyFoodPlacesHandler(IFoodPlaceRepository foodPlaceRepository)
    {
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task<IEnumerable<FoodPlaceDTO>> Handle(GetNearbyFoodPlacesQuery query)
    {
        var foodPlaces = await _foodPlaceRepository.GetNearbyFoodPlaces(query);
        return foodPlaces.Select(fp => new FoodPlaceDTO
        {
            Id = fp.Id,
            Name = fp.Name,
            Description = fp.Description,
            Category = fp.Category,
        });
    }
}
