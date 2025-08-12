public class GetFoodPlaceHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public GetFoodPlaceHandler(IFoodPlaceRepository foodPlaceRepository)
    {
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task<FoodPlaceDTO> Handle(int id)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(id);
        if (foodPlace == null)
        {
            throw new InvalidOperationException($"Food place with ID {id} not found.");
        }

        return new FoodPlaceDTO
        {
            Id = foodPlace.Id,
            Name = foodPlace.Name,
            Description = foodPlace.Description,
            Category = foodPlace.Category,
            Items = foodPlace
                .Items?.Where(item => item.IsAvailable == true)
                .Select(item => new GetFoodPlaceItemDTO
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                }),
        };
    }
}
