public class OrderService
{
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _ordersItemsRepo;
    private readonly QuotesRepository _quotesRepository;
    private readonly QuotesItemsRepository _quotesItemsRepository;

    public OrderService(
        OrdersRepository ordersRepo,
        OrdersItemsRepository ordersItemsRepo,
        QuotesRepository quotesRepository,
        QuotesItemsRepository quotesItemsRepository)
    {
        _ordersRepo = ordersRepo;
        _ordersItemsRepo = ordersItemsRepo;
        _quotesRepository = quotesRepository;
        _quotesItemsRepository = quotesItemsRepository;
    }

    public async Task<int> CreateOrderAsync(int quoteId, QuoteTokenPayload payload)
    {
        var quote = await _quotesRepository.GetQuoteById(quoteId);
        if (quote == null || quote.IsUsed)
            return -1;
        await _quotesRepository.SetQuoteAsUsed(quoteId);

        var quoteItems = await _quotesItemsRepository.GetQuoteItemsByQuoteId(quote.Id);
        if (!quoteItems.Any())
            return 0;

        var orderId = await _ordersRepo.CreateOrder(new CreateOrderDTO { CustomerId = payload.CustomerId, TotalPrice = payload.TotalPrice });
        if (orderId == 0)
            return 0;

        await Task.WhenAll(quoteItems.Select(qItem =>
            _ordersItemsRepo.CreateOrderItem(
                new CreateOrderItemDTO
                {
                    RequestedItem = new RequestedItem { ItemId = qItem.ItemId, Quantity = qItem.Quantity },
                    OrderId = orderId,
                    TotalPrice = qItem.TotalPrice
                }
            )
        ));

        return orderId;
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        return await _ordersRepo.CancelOrder(orderId);
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _ordersRepo.GetOrderById(orderId);
    }
}
