using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly QuoteTokenService _quoteTokenService;

    public OrdersController(OrderService orderService, QuoteTokenService quoteTokenService)
    {
        _orderService = orderService;
        _quoteTokenService = quoteTokenService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_quoteTokenService.ValidateQuoteToken(req.QuoteToken, out var payload))
            return BadRequest();

        var orderId = await _orderService.CreateOrderAsync(req.QuoteId, payload);
        if (orderId == -1)
            return BadRequest();
        if (orderId == 0)
            return StatusCode(500, "Failed to create order");

        return Ok(new OrderResponse { OrderId = orderId });
    }

    [HttpPatch("cancel/{id:int:min(1)}")]
    public async Task<IActionResult> CancelOrder([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool success = await _orderService.CancelOrderAsync(id);
        if (!success)
            return NotFound("Order not found");

        return Ok();
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetOrder([FromRoute] int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(new OrderResponse { OrderId = order.Id, TotalPrice = order.TotalPrice });
    }
}

