using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService service)
    {
        _itemService = service;
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] ItemCreateRequest itemRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int result = await _itemService.CreateItem(itemRequest);
        return Ok(result);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetItem([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        ItemResponse? result = await _itemService.GetItemAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateItem(
        [FromRoute] int id,
        [FromBody] ItemUpdateRequest itemRequest
    )
    {
        if (itemRequest.Id != id)
            return BadRequest("ID in URL must match ID in request body");
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        bool success = await _itemService.UpdateItemAsync(itemRequest);
        if (!success)
            return NotFound("Item not found");
        return Ok();
    }
}
