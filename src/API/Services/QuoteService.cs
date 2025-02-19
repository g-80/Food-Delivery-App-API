public class QuoteService
{
    private readonly QuotesRepository _quotesRepo;
    private readonly QuotesItemsRepository _quotesItemsRepo;
    private readonly PricingService _pricingService;
    private readonly QuoteTokenService _quoteTokenService;

    public QuoteService(
        QuotesRepository quotesRepository,
        QuotesItemsRepository quotesItemsRepository,
        PricingService pricingService,
        QuoteTokenService quoteTokenService)
    {
        _quotesRepo = quotesRepository;
        _quotesItemsRepo = quotesItemsRepository;
        _pricingService = pricingService;
        _quoteTokenService = quoteTokenService;
    }

    public async Task<QuoteResponse> CreateQuoteAsync(CustomerItemsRequest req)
    {
        var expiry = DateTime.UtcNow.AddMinutes(5);
        var (itemsPrices, totalPrice) = await _pricingService.CalculatePriceAsync(req.Items);

        var payload = new QuoteTokenPayload
        {
            CustomerId = req.CustomerId,
            Items = req.Items,
            TotalPrice = totalPrice,
            ExpiresAt = expiry
        };

        string token = _quoteTokenService.GenerateQuoteToken(payload);
        int quoteId = await _quotesRepo.CreateQuote(req.CustomerId, totalPrice, expiry);

        await Task.WhenAll(req.Items.Select((item, i) =>
            _quotesItemsRepo.CreateQuoteItem(
                item,
                quoteId,
                itemsPrices[i]
            )
        ));

        return new QuoteResponse
        {
            QuoteId = quoteId,
            QuoteToken = token,
            QuoteTokenPayload = payload
        };
    }

    public async Task<Quote?> GetQuoteByIdAsync(int id)
    {
        return await _quotesRepo.GetQuoteById(id);
    }

    public async Task<int> SetQuoteAsUsedAsync(int id)
    {
        return await _quotesRepo.SetQuoteAsUsed(id);
    }
}