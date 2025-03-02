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
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _quoteService.CreateQuoteAsync(req);
        return Ok(response);
    }

    [HttpPatch("use/{id:int:min(1)}")]
    public async Task<IActionResult> UseQuote([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool success = await _quoteService.SetQuoteAsUsedAsync(id);
        if (!success)
            return NotFound("Quote not found");
        return Ok();
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetQuote([FromRoute] int id)
    {
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