public class AddItemHandler
{
    private readonly ICartRepository _cartRepo;
    private readonly IFoodPlaceRepository _foodPlaceRepo;
    private readonly ILogger<AddItemHandler> _logger;

    public AddItemHandler(
        ICartRepository cartRepo,
        IFoodPlaceRepository foodPlaceRepo,
        ILogger<AddItemHandler> logger
    )
    {
        _cartRepo = cartRepo;
        _foodPlaceRepo = foodPlaceRepo;
        _logger = logger;
    }

    public async Task Handle(AddItemCommand req, int customerId)
    {
        var cart =
            await _cartRepo.GetCartByCustomerId(customerId)
            ?? throw new Exception($"Cart for customer ID: {customerId} not found.");

        var foodPlace = await _foodPlaceRepo.GetFoodPlaceByItemId(req.ItemId);
        var item = foodPlace!.Items.FirstOrDefault(i => i.Id == req.ItemId);

        cart.AddItem(
            new CartItem
            {
                CartId = cart.Id,
                ItemId = req.ItemId,
                Quantity = req.Quantity,
                UnitPrice = item!.Price,
                Subtotal = req.Quantity * item.Price,
            },
            foodPlace.Id
        );

        if (cart.IsModified)
        {
            await _cartRepo.UpdateCart(cart);
            _logger.LogInformation(
                "Item with ID: {ItemId} added to cart for customer ID: {CustomerId}.",
                req.ItemId,
                customerId
            );
        }
    }
}
