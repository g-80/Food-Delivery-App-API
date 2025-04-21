using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    IUsersRepository _usersRepo;
    IRefreshTokensRepository _refreshTokensRepo;
    IConfiguration _configuration;

    public TokenService(
        IUsersRepository usersRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IConfiguration configuration
    )
    {
        _refreshTokensRepo = refreshTokensRepository;
        _usersRepo = usersRepository;
        _configuration = configuration;
    }

    public async Task<TokenResponse> CreateTokenResponse(User user)
    {
        return new TokenResponse
        {
            AccessToken = CreateAccessToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id),
        };
    }

    public async Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user == null)
            return null;

        return await CreateTokenResponse(user);
    }

    private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _usersRepo.GetUserById(userId);
        if (user == null)
        {
            return null;
        }
        var savedToken = await _refreshTokensRepo.GetRefreshTokenByUserId(userId);
        if (
            savedToken == null
            || savedToken.Token != refreshToken
            || savedToken.ExpiresAt <= DateTime.UtcNow
        )
        {
            return null;
        }

        return user;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
    {
        await DeleteRefreshTokenAsync(userId);

        var refreshToken = GenerateRefreshToken();
        var dto = new RefreshTokenDTO
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        await _refreshTokensRepo.CreateRefreshToken(dto);
        return refreshToken;
    }

    private async Task DeleteRefreshTokenAsync(int userId)
    {
        await _refreshTokensRepo.DeleteRefreshToken(userId);
    }

    private string CreateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.UserType.ToString()),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key")!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("Jwt:Issuer"),
            audience: _configuration.GetValue<string>("Jwt:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
