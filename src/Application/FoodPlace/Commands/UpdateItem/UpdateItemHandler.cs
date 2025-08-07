public class UpdateItemHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public UpdateItemHandler(IFoodPlaceRepository foodPlaceRepository)
    {
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task Handle(UpdateItemCommand req, int userId)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceByUserId(userId);
        if (foodPlace == null)
        {
            throw new Exception("Food place not found");
        }

        var item = new FoodPlaceItem
        {
            Id = req.Id,
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            IsAvailable = req.IsAvailable,
        };

        foodPlace.UpdateItem(item);
        await _foodPlaceRepository.UpdateFoodPlaceItem(item);
    }
}
