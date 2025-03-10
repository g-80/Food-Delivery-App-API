using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var orderId = await _orderService.CreateOrderAsync(req.QuoteId, req.QuoteToken);

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

