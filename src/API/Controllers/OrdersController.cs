using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = nameof(UserTypes.customer))]
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var customerId = GetCustomerIdFromJwt();
        var orderId = await _orderService.CreateOrderAsync(customerId);

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

        return Ok(order);
    }

    private int GetCustomerIdFromJwt()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    }
}
