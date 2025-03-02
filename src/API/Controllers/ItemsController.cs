using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly ItemsRepository _itemsRepo;
    public ItemsController(ItemsRepository repo)
    {
        _itemsRepo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest itemRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int result = await _itemsRepo.CreateItem(itemRequest);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
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

    [HttpPut("{id:int}")]
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