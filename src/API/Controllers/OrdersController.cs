using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = nameof(UserTypes.customer))]
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var tempCustomerId = 1;
        var orderId = await _orderService.CreateOrderAsync(tempCustomerId);

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

