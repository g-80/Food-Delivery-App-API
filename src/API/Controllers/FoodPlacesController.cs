using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/food-places")]
[Authorize]
public class FoodPlacesController : ControllerBase
{
    private readonly CreateFoodPlaceHandler _createFoodPlaceHandler;
    private readonly CreateItemHandler _createItemHandler;
    private readonly UpdateItemHandler _updateItemHandler;
    private readonly GetFoodPlaceHandler _getFoodPlaceHandler;
    private readonly GetNearbyFoodPlacesHandler _getNearbyFoodPlacesHandler;
    private readonly SearchFoodPlacesHandler _searchFoodPlacesHandler;
    private readonly IUserContextService _userContextService;

    public FoodPlacesController(
        CreateFoodPlaceHandler createFoodPlaceHandler,
        CreateItemHandler createItemHandler,
        UpdateItemHandler updateItemHandler,
        GetFoodPlaceHandler getFoodPlaceHandler,
        GetNearbyFoodPlacesHandler getNearbyFoodPlacesHandler,
        SearchFoodPlacesHandler searchFoodPlacesHandler,
        IUserContextService userContextService
    )
    {
        _createFoodPlaceHandler = createFoodPlaceHandler;
        _createItemHandler = createItemHandler;
        _updateItemHandler = updateItemHandler;
        _getFoodPlaceHandler = getFoodPlaceHandler;
        _getNearbyFoodPlacesHandler = getNearbyFoodPlacesHandler;
        _searchFoodPlacesHandler = searchFoodPlacesHandler;
        _userContextService = userContextService;
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetFoodPlacesWithinDistance(
        [FromQuery] GetNearbyFoodPlacesQuery query
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        IEnumerable<FoodPlaceDTO> result = await _getNearbyFoodPlacesHandler.Handle(query);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchFoodPlacesWithinDistance(
        [FromQuery] SearchFoodPlacesQuery query
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        IEnumerable<FoodPlaceDTO> result = await _searchFoodPlacesHandler.Handle(query);
        return Ok(result);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetFoodPlace([FromRoute] int id)
    {
        FoodPlaceDTO? result = await _getFoodPlaceHandler.Handle(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPost]
    public async Task<IActionResult> CreateFoodPlace([FromBody] CreateFoodPlaceCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int userId = _userContextService.GetUserIdFromJwt();
        var res = await _createFoodPlaceHandler.Handle(req, userId);
        return Ok(res);
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int userId = _userContextService.GetUserIdFromJwt();
        await _createItemHandler.Handle(req, userId);
        return Ok();
    }

    [Authorize(Roles = nameof(UserTypes.food_place))]
    [HttpPut("items")]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateItemCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int userId = _userContextService.GetUserIdFromJwt();
        await _updateItemHandler.Handle(req, userId);
        return Ok();
    }
}
