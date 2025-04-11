using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemsRepository _itemsRepo;
    public ItemsController(IItemsRepository repo)
    {
        _itemsRepo = repo;
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest itemRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int result = await _itemsRepo.CreateItem(itemRequest);
        return Ok(result);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetItem([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        Item? result = await _itemsRepo.GetItemById(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateItem([FromRoute] int id, [FromBody] UpdateItemRequest itemRequest)
    {
        if (itemRequest.Id != id)
            return BadRequest("ID in URL must match ID in request body");
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        bool success = await _itemsRepo.UpdateItem(itemRequest);
        if (!success)
            return NotFound("Item not found");
        return Ok();
    }
}