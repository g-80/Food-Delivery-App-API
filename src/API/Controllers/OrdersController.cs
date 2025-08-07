using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly GetOrderHandler _getOrderHandler;
    private readonly GetAllUserOrdersHandler _getAllUserOrdersHandler;
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly CancelOrderHandler _cancelOrderHandler;
    private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
    private readonly IOrderConfirmationService _orderConfirmationService;
    private readonly IUserContextService _userContextService;

    public OrdersController(
        GetOrderHandler getOrderHandler,
        GetAllUserOrdersHandler getAllUserOrdersHandler,
        CreateOrderHandler createOrderHandler,
        CancelOrderHandler cancelOrderHandler,
        UpdateOrderStatusHandler updateOrderStatusHandler,
        IOrderConfirmationService orderConfirmationService,
        IUserContextService userContextService
    )
    {
        _getOrderHandler = getOrderHandler;
        _getAllUserOrdersHandler = getAllUserOrdersHandler;
        _createOrderHandler = createOrderHandler;
        _cancelOrderHandler = cancelOrderHandler;
        _updateOrderStatusHandler = updateOrderStatusHandler;
        _orderConfirmationService = orderConfirmationService;
        _userContextService = userContextService;
    }

    [Authorize(Roles = nameof(UserTypes.customer))]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = _userContextService.GetUserIdFromJwt();
        var orderId = await _createOrderHandler.Handle(request, customerId);

        return Ok(new { orderId });
    }

    [Authorize(Roles = nameof(UserTypes.customer) + "," + nameof(UserTypes.food_place))]
    [HttpPatch("cancel/{orderId:int:min(1)}")]
    public async Task<IActionResult> CancelOrder(
        [FromRoute] int orderId,
        [FromBody] CancelOrderCommand req
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userContextService.GetUserIdFromJwt();
        bool success = await _cancelOrderHandler.Handle(req, userId, orderId);
        if (!success)
            return BadRequest();

        return Ok();
    }

    [HttpGet("{orderId:int:min(1)}")]
    public async Task<IActionResult> GetOrder([FromRoute] int orderId)
    {
        var userId = _userContextService.GetUserIdFromJwt();
        var order = await _getOrderHandler.Handle(orderId, userId);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserOrders()
    {
        var userId = _userContextService.GetUserIdFromJwt();
        var orders = await _getAllUserOrdersHandler.Handle(userId);

        return Ok(orders);
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPatch("status/{orderId:int:min(1)}")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] int orderId,
        [FromBody] UpdateOrderStatusCommand req
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userContextService.GetUserIdFromJwt();
        bool success = await _updateOrderStatusHandler.Handle(req, userId, orderId);
        if (!success)
            return Unauthorized();

        return Ok();
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPatch("confirmation/{orderId:int:min(1)}")]
    public IActionResult ConfirmOrder(
        [FromRoute] int orderId,
        [FromBody] OrderConfirmationRequest request
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.Confirmed)
        {
            _orderConfirmationService.ConfirmOrder(orderId);
        }
        else
        {
            _orderConfirmationService.RejectOrder(orderId);
        }

        return Ok();
    }
}
