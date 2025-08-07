using System.Security.Claims;
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

    public CartsController(
        AddItemHandler addItemHandler,
        RemoveItemHandler removeItemHandler,
        UpdateItemQuantityHandler updateItemQuantityHandler,
        GetCartHandler getCartHandler,
        IUserContextService userContextService
    )
    {
        _addItemHandler = addItemHandler;
        _removeItemHandler = removeItemHandler;
        _updateItemQuantityHandler = updateItemQuantityHandler;
        _getCartHandler = getCartHandler;
        _userContextService = userContextService;
    }

    [HttpPost]
    public async Task<IActionResult> AddCartItem([FromBody] AddItemCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = _userContextService.GetUserIdFromJwt();
        await _addItemHandler.Handle(req, customerId);
        return Ok();
    }

    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> RemoveCartItem([FromRoute] int id)
    {
        var customerId = _userContextService.GetUserIdFromJwt();
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
