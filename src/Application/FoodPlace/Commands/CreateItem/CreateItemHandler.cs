public class CreateItemHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public CreateItemHandler(IFoodPlaceRepository foodPlaceRepository)
    {
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task Handle(CreateItemCommand req, int userId)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceByUserId(userId);
        if (foodPlace == null)
        {
            throw new Exception("Food place not found");
        }

        var item = new FoodPlaceItem
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            IsAvailable = req.IsAvailable,
        };

        foodPlace.AddItem(item);
        await _foodPlaceRepository.AddFoodPlaceItem(foodPlace.Id, item);
    }
}
