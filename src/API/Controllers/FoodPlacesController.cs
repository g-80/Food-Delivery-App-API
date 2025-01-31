using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/food-places")]
public class FoodPlacesController : ControllerBase
{
    private readonly FoodPlacesRepository _foodPlacesRepo;
    public FoodPlacesController(FoodPlacesRepository repo)
    {
        _foodPlacesRepo = repo;
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetFoodPlacesWithinDistance([FromQuery] NearbyFoodPlacesRequest query)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        IEnumerable<FoodPlace> result = await _foodPlacesRepo.GetFoodPlacesWithinDistance(query);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchFoodPlacesWithinDistance([FromQuery] SearchFoodPlacesRequest query)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        IEnumerable<FoodPlace> result = await _foodPlacesRepo.SearchFoodPlacesWithinDistance(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFoodPlace([FromRoute] int id)
    {
        if (id <= 0)
        {
            ModelState.AddModelError("id", "Invalid id");
            return BadRequest(ModelState);
        }
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        FoodPlace? result = await _foodPlacesRepo.GetFoodPlace(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}