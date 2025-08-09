using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/carts")]
[Authorize(Roles = nameof(UserTypes.customer))]
public class CartsController : ControllerBase
{
    private readonly AddItemHandler _addItemHandler;
    private readonly RemoveItemHandler _removeItemHandler;
    private readonly UpdateItemQuantityHandler _updateItemQuantityHandler;
    private readonly GetCartHandler _getCartHandler;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<CartsController> _logger;

    public CartsController(
        AddItemHandler addItemHandler,
        RemoveItemHandler removeItemHandler,
        UpdateItemQuantityHandler updateItemQuantityHandler,
        GetCartHandler getCartHandler,
        IUserContextService userContextService,
        ILogger<CartsController> logger
    )
    {
        _addItemHandler = addItemHandler;
        _removeItemHandler = removeItemHandler;
        _updateItemQuantityHandler = updateItemQuantityHandler;
        _getCartHandler = getCartHandler;
        _userContextService = userContextService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddCartItem([FromBody] AddItemCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = _userContextService.GetUserIdFromJwt();
        using var scope = _logger.BeginScope(
            "CorrelationId: {CorrelationId}",
            HttpContext.TraceIdentifier
        );
        _logger.LogInformation(
            "Received request to add item with ID: {ItemId} to cart for customer ID: {CustomerId}",
            req.ItemId,
            customerId
        );
        await _addItemHandler.Handle(req, customerId);
        return Ok();
    }

    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> RemoveCartItem([FromRoute] int id)
    {
        var customerId = _userContextService.GetUserIdFromJwt();
        using var scope = _logger.BeginScope(
            "CorrelationId: {CorrelationId}",
            HttpContext.TraceIdentifier
        );
        _logger.LogInformation(
            "Received request to remove item with ID: {ItemId} from cart for customer ID: {CustomerId}",
            id,
            customerId
        );
        await _removeItemHandler.Handle(id, customerId);
        return Ok();
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateCartItemQuantity(
        [FromBody] UpdateItemQuantityCommand req
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = _userContextService.GetUserIdFromJwt();
        using var scope = _logger.BeginScope(
            "CorrelationId: {CorrelationId}",
            HttpContext.TraceIdentifier
        );
        _logger.LogInformation(
            "Received request to update item quantity for item with ID: {ItemId} customer ID: {CustomerId}",
            req.ItemId,
            customerId
        );
        await _updateItemQuantityHandler.Handle(req, customerId);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var customerId = _userContextService.GetUserIdFromJwt();
        CartDTO? result = await _getCartHandler.Handle(customerId);
        if (result == null)
        {
            return Ok();
        }
        return Ok(result);
    }
}
