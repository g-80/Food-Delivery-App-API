public class SearchFoodPlacesHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public SearchFoodPlacesHandler(IFoodPlaceRepository foodPlaceRepository)
    {
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task<IEnumerable<FoodPlaceDTO>> Handle(SearchFoodPlacesQuery query)
    {
        var foodPlaces = await _foodPlaceRepository.SearchFoodPlacesWithinDistance(query);
        return foodPlaces.Select(fp => new FoodPlaceDTO
        {
            Id = fp.Id,
            Name = fp.Name,
            Description = fp.Description,
            Category = fp.Category,
        });
    }
}
