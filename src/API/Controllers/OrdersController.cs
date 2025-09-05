using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly GetOrderHandler _getOrderHandler;
    private readonly GetAllUserOrdersHandler _getAllUserOrdersHandler;
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly CancelOrderHandler _cancelOrderHandler;
    private readonly ProcessOrderHandler _processOrderHandler;
    private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
    private readonly IOrderConfirmationService _orderConfirmationService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<OrdersController> _logger;
    private readonly IConfiguration _config;

    public OrdersController(
        GetOrderHandler getOrderHandler,
        GetAllUserOrdersHandler getAllUserOrdersHandler,
        CreateOrderHandler createOrderHandler,
        CancelOrderHandler cancelOrderHandler,
        UpdateOrderStatusHandler updateOrderStatusHandler,
        ProcessOrderHandler processOrderHandler,
        IOrderConfirmationService orderConfirmationService,
        IUserContextService userContextService,
        ILogger<OrdersController> logger,
        IConfiguration configuration
    )
    {
        _getOrderHandler = getOrderHandler;
        _getAllUserOrdersHandler = getAllUserOrdersHandler;
        _createOrderHandler = createOrderHandler;
        _cancelOrderHandler = cancelOrderHandler;
        _updateOrderStatusHandler = updateOrderStatusHandler;
        _processOrderHandler = processOrderHandler;
        _orderConfirmationService = orderConfirmationService;
        _userContextService = userContextService;
        _logger = logger;
        _config = configuration;
    }

    [Authorize(Roles = nameof(UserTypes.customer))]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand req)
    {
        var customerId = _userContextService.GetUserIdFromJwt();
        using var scope = _logger.BeginScope(
            "CorrelationId: {CorrelationId}",
            HttpContext.TraceIdentifier
        );
        _logger.LogInformation(
            "Received request to create order and payment intent for customer ID: {CustomerId}",
            customerId
        );

        var resultDto = await _createOrderHandler.Handle(customerId, req);

        return Ok(resultDto);
    }

    [HttpPost("webhook/stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        string endpointSecret = _config.GetValue<string>("Stripe:WebhookSecret")!;
        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);
            var signatureHeader = Request.Headers["Stripe-Signature"];

            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogInformation(
                    "A successful payment for {paymentIntentId} was made with amount {amount}",
                    paymentIntent!.Id,
                    paymentIntent.Amount
                );

                int orderId = int.Parse(paymentIntent.Metadata["order_id"]);
                using var scope = _logger.BeginScope(
                    "CorrelationId: {CorrelationId}",
                    HttpContext.TraceIdentifier
                );
                await _processOrderHandler.Handle(orderId);

                return Ok();
            }
            else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogInformation(
                    "Payment failed for {paymentIntentId} for amount {amount}",
                    paymentIntent!.Id,
                    paymentIntent.Amount
                );
            }
            else
            {
                _logger.LogInformation("Unhandled event type: {eventType}", stripeEvent.Type);
            }
            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError("Stripe webhook error: {errorMessage}", e.Message);
            return BadRequest();
        }
        catch
        {
            return StatusCode(500);
        }
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
        _logger.LogInformation(
            "Received request to cancel order ID: {OrderId} from user ID: {UserId}",
            orderId,
            userId
        );
        bool success = await _cancelOrderHandler.Handle(req, userId, orderId);
        if (!success)
            return Unauthorized();

        return Ok();
    }

    [Authorize(Roles = nameof(UserTypes.customer) + "," + nameof(UserTypes.food_place))]
    [HttpGet("{orderId:int:min(1)}")]
    public async Task<IActionResult> GetOrder([FromRoute] int orderId)
    {
        var userId = _userContextService.GetUserIdFromJwt();
        var order = await _getOrderHandler.Handle(orderId, userId);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [Authorize(Roles = nameof(UserTypes.customer))]
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
        _logger.LogInformation(
            "Received request to update status to {OrderStatus} for order ID: {OrderId}",
            orderId,
            req.Status
        );
        bool success = await _updateOrderStatusHandler.Handle(req, userId, orderId);
        if (!success)
            return Unauthorized();

        return Ok();
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPatch("confirmation/{orderId:int:min(1)}")]
    public async Task<IActionResult> ConfirmOrder(
        [FromRoute] int orderId,
        [FromBody] OrderConfirmationRequest request
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _userContextService.GetUserIdFromJwt();
        bool success;

        if (request.Confirmed)
        {
            _logger.LogInformation(
                "Received confirmation for order ID: {OrderId} from user ID: {UserId}",
                orderId,
                userId
            );
            success = await _orderConfirmationService.ConfirmOrder(orderId, userId);
        }
        else
        {
            _logger.LogInformation(
                "Received rejection for order ID: {OrderId} from user ID: {UserId}",
                orderId,
                userId
            );
            success = await _orderConfirmationService.RejectOrder(orderId, userId);
        }

        if (!success)
            return Unauthorized();

        return Ok();
    }
}
