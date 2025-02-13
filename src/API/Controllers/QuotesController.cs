using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly QuotesRepository _quotesRepo;
    private readonly QuotesItemsRepository _quotesItemsRepo;
    private readonly PricingService _pricingService;
    private readonly QuoteTokenService _quoteTokenService;
    public QuotesController(QuotesRepository quotesRepo, QuotesItemsRepository quotesItemsRepo, PricingService pricingService, QuoteTokenService quoteTokenService)
    {
        _quotesRepo = quotesRepo;
        _quotesItemsRepo = quotesItemsRepo;
        _pricingService = pricingService;
        _quoteTokenService = quoteTokenService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuote([FromBody] CustomerItemsRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        DateTime expiry = DateTime.UtcNow.AddMinutes(5);
        var (itemsPrice, totalPrice) = await _pricingService.CalculatePriceAsync(req.Items);
        QuoteTokenPayload payload = new()
        {
            UserId = req.CustomerId,
            Items = req.Items,
            TotalPrice = totalPrice,
            ExpiresAt = expiry
        };
        string token = _quoteTokenService.GenerateQuoteToken(payload);
        int quoteId = await _quotesRepo.CreateQuote(req, totalPrice, expiry);
        for (int i = 0; i < req.Items.Count; i++)
        {
            await _quotesItemsRepo.CreateQuoteItem(req.Items[i], quoteId, itemsPrice[i]);
        }
        QuoteResponse res = new() { QuoteId = quoteId, QuoteToken = token, QuoteTokenPayload = payload };
        return Ok(res);
    }

    [HttpPatch("use/{id:int}")]
    public async Task<IActionResult> UseQuote([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int _ = await _quotesRepo.SetQuoteAsUsed(id);
        return Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetQuote([FromRoute] int id)
    {
        if (id <= 0)
        {
            ModelState.AddModelError("id", "Invalid id");
            return BadRequest(ModelState);
        }
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        Quote? result = await _quotesRepo.GetQuoteById(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}