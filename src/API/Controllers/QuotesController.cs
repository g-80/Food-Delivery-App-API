using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly QuoteService _quoteService;

    public QuotesController(QuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuote([FromBody] CustomerItemsRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _quoteService.CreateQuoteAsync(req);
        return Ok(response);
    }

    [HttpPatch("use/{id:int}")]
    public async Task<IActionResult> UseQuote([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int _ = await _quoteService.SetQuoteAsUsedAsync(id);
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
        Quote? result = await _quoteService.GetQuoteByIdAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}