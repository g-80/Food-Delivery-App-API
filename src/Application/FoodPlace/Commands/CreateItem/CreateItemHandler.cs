public class CreateItemHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly ILogger<CreateItemHandler> _logger;

    public CreateItemHandler(
        IFoodPlaceRepository foodPlaceRepository,
        ILogger<CreateItemHandler> logger
    )
    {
        _foodPlaceRepository = foodPlaceRepository;
        _logger = logger;
    }

    public async Task Handle(CreateItemCommand req, int userId)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceByUserId(userId);
        if (foodPlace == null)
        {
            _logger.LogError("Food place not found for user ID: {UserId}", userId);
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
        _logger.LogInformation(
            "Item {ItemId} created successfully for food place ID: {FoodPlaceId} by user ID: {UserId}",
            item.Id,
            foodPlace.Id,
            userId
        );
    }
}
