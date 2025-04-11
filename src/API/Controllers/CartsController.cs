using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/carts")]
[Authorize(Roles = nameof(UserTypes.customer))]
public class CartsController : ControllerBase
{
    private readonly CartService _cartService;

    public CartsController(CartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddCartItem([FromBody] AddItemToCartRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _cartService.AddItemToCartAsync(req);
        return Ok();
    }

    [HttpDelete("items/{id:int:min(1)}")]
    public async Task<IActionResult> RemoveCartItem([FromRoute] int id)
    {
        var tempCustomerId = 1;
        await _cartService.RemoveItemFromCartAsync(tempCustomerId, id);
        return Ok();
    }

    [HttpPatch("items/{id:int:min(1)}")]
    public async Task<IActionResult> UpdateCartItemQuantity([FromRoute] int id, [FromBody] UpdateCartItemQuantityRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tempCustomerId = 1;
        await _cartService.UpdateCartItemQuantityAsync(tempCustomerId, id, req);
        return Ok();
    }

    [HttpGet("{customerId:int:min(1)}")]
    public async Task<IActionResult> GetCart([FromRoute] int customerId)
    {
        CartResponse? result = await _cartService.GetCartDetailsAsync(customerId);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}