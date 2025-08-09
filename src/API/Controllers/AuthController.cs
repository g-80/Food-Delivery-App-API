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
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        GetUserHandler getUserHandler,
        SignUpUserHandler signUpUserHandler,
        LogInUserHandler logInUserHandler,
        UpdateUserHandler updateUserHandler,
        RenewAccessTokenHandler renewAccessTokenHandler,
        ILogger<AuthController> logger
    )
    {
        _getUserHandler = getUserHandler;
        _signUpUserHandler = signUpUserHandler;
        _logInUserHandler = logInUserHandler;
        _updateUserHandler = updateUserHandler;
        _renewAccessTokenHandler = renewAccessTokenHandler;
        _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUpUser([FromBody] SignUpUserCommand req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using var scope = _logger.BeginScope(
            "CorrelationId: {CorrelationId}",
            HttpContext.TraceIdentifier
        );
        _logger.LogInformation(
            "Received request to sign up user with phone number: {PhoneNumber}",
            req.PhoneNumber
        );
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

        _logger.LogInformation(
            "Received request to log in user with phone number: {PhoneNumber}",
            req.PhoneNumber
        );
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

        _logger.LogInformation(
            "Received request to renew access token for refresh token: {RefreshToken}",
            req.RefreshToken
        );
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using var scope = _logger.BeginScope(
            "CorrelationId: {CorrelationId}",
            HttpContext.TraceIdentifier
        );
        _logger.LogInformation("Received request to update user with ID: {UserId}", id);
        await _updateUserHandler.Handle(req, id);
        return Ok();
    }
}
