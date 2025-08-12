public class UpdateItemHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly ILogger<UpdateItemHandler> _logger;

    public UpdateItemHandler(
        IFoodPlaceRepository foodPlaceRepository,
        ILogger<UpdateItemHandler> logger
    )
    {
        _foodPlaceRepository = foodPlaceRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateItemCommand req, int userId)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceByUserId(userId);

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
        _logger.LogInformation(
            "Item {ItemId} updated successfully for food place ID: {FoodPlaceId} by user ID: {UserId}",
            item.Id,
            foodPlace.Id,
            userId
        );
    }
}
