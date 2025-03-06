public class TestDataSeeder
{
    private readonly FoodPlacesRepository _foodPlacesRepo;
    private readonly ItemsRepository _itemsRepo;
    private readonly QuotesRepository _quotesRepo;
    private readonly QuotesItemsRepository _quotesItemsRepo;
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _orderItemsRepo;
    public TestDataSeeder(FoodPlacesRepository foodPlacesRepo, ItemsRepository itemsRepo, QuotesRepository quotesRepo, QuotesItemsRepository quotesItemsRepo, OrdersRepository ordersRepo, OrdersItemsRepository orderItemsRepo)
    {
        _foodPlacesRepo = foodPlacesRepo;
        _itemsRepo = itemsRepo;
        _quotesRepo = quotesRepo;
        _quotesItemsRepo = quotesItemsRepo;
        _ordersRepo = ordersRepo;
        _orderItemsRepo = orderItemsRepo;
    }
    public async Task SeedFoodPlaces()
    {
        await Task.WhenAll(TestData.FoodPlaces.foodPlacesFixtures.Select(foodPlace => _foodPlacesRepo.CreateFoodPlace(foodPlace)));
    }

    public async Task SeedItems()
    {
        await Task.WhenAll(TestData.Items.defaults.Select(async item => TestData.Items.assignedIds.Add(await _itemsRepo.CreateItem(item))));
    }

    public async Task<int> SeedQuoteAndQuoteItems()
    {
        var quoteId = await _quotesRepo.CreateQuote(TestData.Orders.CreateQuoteDTO());
        await Task.WhenAll(TestData.Orders.itemRequests.Select((item, i) =>
            _quotesItemsRepo.CreateQuoteItem(
                new CreateQuoteItemDTO
                {
                    RequestedItem = item,
                    QuoteId = quoteId,
                    TotalPrice = TestData.Orders.prices[i]
                }
            )
        ));
        return quoteId;
    }

    public async Task<int> SeedOrderAndOrderItems()
    {
        var orderId = await _ordersRepo.CreateOrder(TestData.Orders.CreateOrderDTO());
        await Task.WhenAll(TestData.Orders.itemRequests.Select((item, i) =>
            _orderItemsRepo.CreateOrderItem(
                new CreateOrderItemDTO
                {
                    RequestedItem = item,
                    OrderId = orderId,
                    TotalPrice = TestData.Orders.prices[i]
                }
            )
        ));
        return orderId;
    }
}