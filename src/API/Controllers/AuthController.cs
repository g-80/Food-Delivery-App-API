using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GetUserHandler _getUserHandler;
    private readonly SignUpUserHandler _signUpUserHandler;
    private readonly LogInUserHandler _logInUserHandler;
    private readonly UpdateUserHandler _updateUserHandler;
    private readonly RenewAccessTokenHandler _renewAccessTokenHandler;

    public AuthController(
        GetUserHandler getUserHandler,
        SignUpUserHandler signUpUserHandler,
        LogInUserHandler logInUserHandler,
        UpdateUserHandler updateUserHandler,
        RenewAccessTokenHandler renewAccessTokenHandler
    )
    {
        _getUserHandler = getUserHandler;
        _signUpUserHandler = signUpUserHandler;
        _logInUserHandler = logInUserHandler;
        _updateUserHandler = updateUserHandler;
        _renewAccessTokenHandler = renewAccessTokenHandler;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUpUser([FromBody] SignUpUserCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = await _signUpUserHandler.Handle(req);
        if (userId == null)
            return BadRequest("User already exists");

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LogInUserCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _logInUserHandler.Handle(req);
        if (token == null)
            return BadRequest("Invalid phone number or password");

        return Ok(token);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RenewAccessTokenCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _renewAccessTokenHandler.Handle(req);
        if (result == null || result.AccessToken == null || result.RefreshToken == null)
            return Unauthorized("Invalid refresh token");

        return Ok(result);
    }

    // move to a user controller and add authorisation check
    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _getUserHandler.Handle(id);
        return Ok(result);
    }

    [Authorize]
    [HttpPatch("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand req, int id)
    {
        await _updateUserHandler.Handle(req, id);
        return Ok();
    }
}
