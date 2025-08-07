public class AddItemHandler
{
    private readonly ICartRepository _cartRepo;
    private readonly IFoodPlaceRepository _foodPlaceRepo;

    public AddItemHandler(ICartRepository cartRepo, IFoodPlaceRepository foodPlaceRepo)
    {
        _cartRepo = cartRepo;
        _foodPlaceRepo = foodPlaceRepo;
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
        }
    }
}
