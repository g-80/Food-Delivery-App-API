public class GetCartHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public GetCartHandler(ICartRepository cartRepository, IFoodPlaceRepository foodPlaceRepository)
    {
        _cartRepository = cartRepository;
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task<CartDTO?> Handle(int customerId)
    {
        var cart =
            await _cartRepository.GetCartByCustomerId(customerId)
            ?? throw new Exception($"Cart for customer ID: {customerId} not found.");

        if (!cart.Items.Any())
        {
            return null;
        }

        var foodPlace =
            await _foodPlaceRepository.GetFoodPlaceById(cart.FoodPlaceId)
            ?? throw new Exception($"Food place with ID: {cart.FoodPlaceId} not found.");

        return new CartDTO
        {
            FoodPlaceId = foodPlace.Id,
            FoodPlaceName = foodPlace.Name,
            Items = cart.Items.Select(item => new CartItemDTO
            {
                ItemId = item.ItemId,
                ItemName = foodPlace.Items.FirstOrDefault(fi => fi.Id == item.ItemId)!.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal,
            }),
            Subtotal = cart.Pricing.Subtotal,
            Fees = cart.Pricing.ServiceFee,
            DeliveryFee = cart.Pricing.DeliveryFee,
            Total = cart.Pricing.Total,
        };
    }
}
