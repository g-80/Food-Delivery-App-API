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

    [HttpPut]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateItemRequest itemRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int _ = await _itemsRepo.UpdateItem(itemRequest);
        return Ok();
    }
}