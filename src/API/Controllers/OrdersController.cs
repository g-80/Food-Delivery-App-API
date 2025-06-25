using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly OrderConfirmationService _orderConfirmationService;
    private readonly OrderProcessingOrchestrator _orderProcessingOrchestrator;

    public OrdersController(
        IOrderService orderService,
        OrderConfirmationService orderConfirmationService,
        OrderProcessingOrchestrator orderProcessingOrchestrator
    )
    {
        _orderService = orderService;
        _orderConfirmationService = orderConfirmationService;
        _orderProcessingOrchestrator = orderProcessingOrchestrator;
    }

    [Authorize(Roles = nameof(UserTypes.customer))]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
    {
        var customerId = GetUserIdFromJwt();
        var orderIds = await _orderProcessingOrchestrator.CreateOrderAsync(customerId, request);

        return Ok(new { orderIds });
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [Authorize(Roles = nameof(UserTypes.customer))]
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
        var order = await _orderService.GetOrderResponseByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPatch("status/{id:int:min(1)}")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] int id,
        [FromBody] OrderStatusUpdateRequest request
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool success = await _orderService.UpdateOrderStatusAsync(id, request.Status);
        if (!success)
            return NotFound("Order not found or not authorized to update");

        return Ok();
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPatch("confirmation/{id:int:min(1)}")]
    public IActionResult ConfirmOrder(
        [FromRoute] int id,
        [FromBody] OrderConfirmationRequest request
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.Confirmed)
        {
            _orderConfirmationService.ConfirmOrderAsync(id);
        }
        else
        {
            _orderConfirmationService.RejectOrderAsync(id);
        }

        return Ok();
    }

    private int GetUserIdFromJwt()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    }
}
