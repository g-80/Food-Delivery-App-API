public class OrderService
{
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _ordersItemsRepo;
    private readonly QuotesRepository _quotesRepository;
    private readonly QuotesItemsRepository _quotesItemsRepository;
    private readonly QuoteTokenService _quoteTokenService;

    public OrderService(
        OrdersRepository ordersRepo,
        OrdersItemsRepository ordersItemsRepo,
        QuotesRepository quotesRepository,
        QuotesItemsRepository quotesItemsRepository,
        QuoteTokenService quoteTokenService)
    {
        _ordersRepo = ordersRepo;
        _ordersItemsRepo = ordersItemsRepo;
        _quotesRepository = quotesRepository;
        _quotesItemsRepository = quotesItemsRepository;
        _quoteTokenService = quoteTokenService;
    }

    public async Task<int> CreateOrderAsync(int quoteId, string quoteToken)
    {
        if (!_quoteTokenService.ValidateQuoteToken(quoteToken, out var payload))
            throw new InvalidQuoteTokenException();
        var quote = await _quotesRepository.GetQuoteById(quoteId);
        if (quote == null || quote.IsUsed)
            throw new QuoteNotFoundException();

        await _quotesRepository.SetQuoteAsUsed(quoteId);
        var quoteItems = await _quotesItemsRepository.GetQuoteItemsByQuoteId(quote.Id);

        if (!quoteItems.Any())
            throw new EmptyQuoteException();

        int orderId;
        orderId = await _ordersRepo.CreateOrder(new CreateOrderDTO
        {
            CustomerId = payload.CustomerId,
            TotalPrice = payload.TotalPrice
        });


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
