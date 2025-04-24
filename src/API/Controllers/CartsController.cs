using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/carts")]
[Authorize(Roles = nameof(UserTypes.customer))]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddCartItem([FromBody] CartAddItemRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = GetCustomerIdFromJwt();
        await _cartService.AddItemToCartAsync(customerId, req);
        return Ok();
    }

    [HttpDelete("items/{id:int:min(1)}")]
    public async Task<IActionResult> RemoveCartItem([FromRoute] int id)
    {
        var customerId = GetCustomerIdFromJwt();
        await _cartService.RemoveItemFromCartAsync(customerId, id);
        return Ok();
    }

    [HttpPatch("items/{id:int:min(1)}")]
    public async Task<IActionResult> UpdateCartItemQuantity(
        [FromRoute] int id,
        [FromBody] CartUpdateItemQuantityRequest req
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = GetCustomerIdFromJwt();
        await _cartService.UpdateCartItemQuantityAsync(customerId, id, req);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var customerId = GetCustomerIdFromJwt();
        CartResponse? result = await _cartService.GetCartDetailsAsync(customerId);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    private int GetCustomerIdFromJwt()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    }
}
