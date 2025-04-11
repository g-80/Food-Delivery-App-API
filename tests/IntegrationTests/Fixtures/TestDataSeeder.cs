public class TestDataSeeder
{
    private readonly FoodPlacesRepository _foodPlacesRepo;
    private readonly IItemsRepository _itemsRepo;
    private readonly CartsRepository _cartsRepo;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly CartPricingsRepository _cartPricingsRepo;
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _orderItemsRepo;
    private readonly AuthService _authService;
    public TestDataSeeder(FoodPlacesRepository foodPlacesRepo, IItemsRepository itemsRepo, CartsRepository cartsRepo, ICartItemsRepository cartItemsRepo, CartPricingsRepository cartPricingsRepo, OrdersRepository ordersRepo, OrdersItemsRepository orderItemsRepo, AuthService authService)
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
        await Task.WhenAll(TestData.FoodPlaces.foodPlacesFixtures.Select(foodPlace => _foodPlacesRepo.CreateFoodPlace(foodPlace)));
    }

    public async Task SeedItems()
    {
        await Task.WhenAll(TestData.Items.defaults.Select(async item => TestData.Items.assignedIds.Add(await _itemsRepo.CreateItem(item))));
    }

    public async Task SeedCartData()
    {
        TestData.Carts.assignedCartId = await _cartsRepo.CreateCart(TestData.Carts.CreateCartDTO());
        await Task.WhenAll(TestData.Carts.CreateCartItemDTOs(TestData.Carts.assignedCartId).Select(dto =>
            _cartItemsRepo.CreateCartItem(dto)
        ));
        await _cartPricingsRepo.CreateCartPricing(TestData.Carts.CreateCartPricingDTO(TestData.Carts.assignedCartId));
    }

    public async Task<int> SeedOrderAndOrderItems()
    {
        var orderId = await _ordersRepo.CreateOrder(TestData.Orders.CreateOrderDTO());
        await Task.WhenAll(TestData.Carts.itemRequests.Select((item, i) =>
            _orderItemsRepo.CreateOrderItem(
                new CreateOrderItemDTO
                {
                    RequestedItem = item,
                    OrderId = orderId,
                    Subtotal = TestData.Carts.prices[i]
                }
            )
        ));
        return orderId;
    }

    public async Task SeedUsers()
    {
        await Task.WhenAll(TestData.Users.createUserRequests.Select(async req =>
        {
            int id = (await _authService.RegisterUserAsync(req))!.Id!.Value;
        }));
    }
}