using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/food-places")]
[Authorize]
public class FoodPlacesController : ControllerBase
{
    private readonly IFoodPlacesService _foodPlacesService;

    public FoodPlacesController(IFoodPlacesService service)
    {
        _foodPlacesService = service;
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetFoodPlacesWithinDistance(
        [FromQuery] NearbyFoodPlacesRequest query
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        IEnumerable<FoodPlace> result = await _foodPlacesService.GetFoodPlacesWithinDistance(query);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchFoodPlacesWithinDistance(
        [FromQuery] SearchFoodPlacesRequest query
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        IEnumerable<FoodPlace> result = await _foodPlacesService.SearchFoodPlacesWithinDistance(
            query
        );
        return Ok(result);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetFoodPlace([FromRoute] int id)
    {
        FoodPlace? result = await _foodPlacesService.GetFoodPlaceAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}
