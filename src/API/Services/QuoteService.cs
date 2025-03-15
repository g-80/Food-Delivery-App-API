public class QuoteService
{
    private readonly QuotesRepository _quotesRepo;
    private readonly QuotesItemsRepository _quotesItemsRepo;
    private readonly PricingService _pricingService;
    private readonly QuoteTokenService _quoteTokenService;
    private readonly UnitOfWork _unitOfWork;

    public QuoteService(
        QuotesRepository quotesRepository,
        QuotesItemsRepository quotesItemsRepository,
        PricingService pricingService,
        QuoteTokenService quoteTokenService,
        UnitOfWork unitOfWork)
    {
        _quotesRepo = quotesRepository;
        _quotesItemsRepo = quotesItemsRepository;
        _pricingService = pricingService;
        _quoteTokenService = quoteTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuoteResponse> CreateQuoteAsync(CreateQuoteRequest req)
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
        using (_unitOfWork)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                int quoteId = await _quotesRepo.CreateQuote(new CreateQuoteDTO() { CustomerId = req.CustomerId, TotalPrice = totalPrice, Expiry = expiry }, _unitOfWork.Transaction);

                for (int i = 0; i < req.Items.Count; i++)
                {
                    var item = req.Items[i];
                    await _quotesItemsRepo.CreateQuoteItem(
                        new CreateQuoteItemDTO
                        {
                            RequestedItem = item,
                            QuoteId = quoteId,
                            TotalPrice = itemsPrices[i]
                        },
                        _unitOfWork.Transaction
                    );
                }

                _unitOfWork.Commit();
                return new QuoteResponse
                {
                    QuoteId = quoteId,
                    QuoteToken = token,
                    QuoteTokenPayload = payload
                };

            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }

    public async Task<Quote?> GetQuoteByIdAsync(int id)
    {
        return await _quotesRepo.GetQuoteById(id);
    }

    public async Task<bool> SetQuoteAsUsedAsync(int id)
    {
        return await _quotesRepo.SetQuoteAsUsed(id);
    }
}