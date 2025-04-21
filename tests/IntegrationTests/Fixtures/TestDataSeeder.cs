public class TestDataSeeder
{
    private readonly IFoodPlacesRepository _foodPlacesRepo;
    private readonly IItemsRepository _itemsRepo;
    private readonly ICartsRepository _cartsRepo;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly ICartPricingsRepository _cartPricingsRepo;
    private readonly IOrdersRepository _ordersRepo;
    private readonly IOrdersItemsRepository _orderItemsRepo;
    private readonly AuthService _authService;

    public TestDataSeeder(
        IFoodPlacesRepository foodPlacesRepo,
        IItemsRepository itemsRepo,
        ICartsRepository cartsRepo,
        ICartItemsRepository cartItemsRepo,
        ICartPricingsRepository cartPricingsRepo,
        IOrdersRepository ordersRepo,
        IOrdersItemsRepository orderItemsRepo,
        AuthService authService
    )
    {
        _foodPlacesRepo = foodPlacesRepo;
        _itemsRepo = itemsRepo;
        _cartsRepo = cartsRepo;
        _cartItemsRepo = cartItemsRepo;
        _cartPricingsRepo = cartPricingsRepo;
        _ordersRepo = ordersRepo;
        _orderItemsRepo = orderItemsRepo;
        _authService = authService;
    }

    public async Task SeedFoodPlaces()
    {
        foreach (var foodPlace in TestData.FoodPlaces.foodPlacesFixtures)
        {
            int foodPlaceId = await _foodPlacesRepo.CreateFoodPlace(foodPlace);
            TestData.FoodPlaces.assignedIds.Add(foodPlaceId);
        }
    }

    public async Task SeedItems()
    {
        foreach (var item in TestData.Items.defaults)
        {
            int itemId = await _itemsRepo.CreateItem(item);
            TestData.Items.assignedIds.Add(itemId);
        }
    }

    public async Task SeedCartItems()
    {
        TestData.Carts.assignedCartId = (
            await _cartsRepo.GetCartByCustomerId(TestData.Users.assignedIds[0])
        )!.Id;
        await Task.WhenAll(
            TestData
                .Carts.CreateCartItemDTOs(TestData.Carts.assignedCartId)
                .Select(dto => _cartItemsRepo.CreateCartItem(dto))
        );
        await _cartPricingsRepo.UpdateCartPricing(
            TestData.Carts.CreateCartPricingDTO(TestData.Carts.assignedCartId)
        );
    }

    public async Task SeedOrderAndOrderItems()
    {
        var orderId = await _ordersRepo.CreateOrder(TestData.Orders.CreateOrderDTO());
        TestData.Orders.assignedIds.Add(orderId);
        await Task.WhenAll(
            TestData.Carts.itemRequests.Select(
                (item, i) =>
                    _orderItemsRepo.CreateOrderItem(
                        new CreateOrderItemDTO
                        {
                            RequestedItem = item,
                            OrderId = orderId,
                            Subtotal = TestData.Carts.prices[i],
                        }
                    )
            )
        );
    }

    public async Task SeedUsers()
    {
        foreach (var req in TestData.Users.createUserRequests)
        {
            int userId = (int)await _authService.SignUpUserAsync(req);
            TestData.Users.assignedIds.Add(userId);
        }
    }
}
