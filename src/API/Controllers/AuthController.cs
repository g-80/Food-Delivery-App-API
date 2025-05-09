using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;

    public AuthController(AuthService authService, TokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUpUser([FromBody] UserCreateRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = await _authService.SignUpUserAsync(req);
        if (userId == null)
            return BadRequest("User already exists");

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _authService.LoginAsync(req);
        if (token == null)
            return BadRequest("Invalid phone number or password");

        return Ok(token);
    }

    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _tokenService.RefreshTokenAsync(req);
        if (result == null || result.AccessToken == null || result.RefreshToken == null)
            return Unauthorized("Invalid refresh token");

        return Ok(result);
    }
}
