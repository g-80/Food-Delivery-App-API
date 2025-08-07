using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
    IUserRepository _usersRepo;
    IRefreshTokensRepository _refreshTokensRepo;
    IConfiguration _configuration;

    public TokenService(
        IUserRepository usersRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IConfiguration configuration
    )
    {
        _refreshTokensRepo = refreshTokensRepository;
        _usersRepo = usersRepository;
        _configuration = configuration;
    }

    public async Task<AuthTokenResponse> GenerateTokens(string userId, string userType)
    {
        return new AuthTokenResponse
        {
            AccessToken = CreateAccessToken(userId, userType),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(int.Parse(userId)),
        };
    }

    public async Task<AuthTokenResponse?> RenewAccessToken(RenewAccessTokenCommand request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user == null)
            return null;

        return await GenerateTokens(user.Id.ToString(), user.UserType.ToString());
    }

    private string CreateAccessToken(string userId, string userType)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, userType),
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

    private async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
    {
        await DeleteRefreshTokenAsync(userId);

        var refreshToken = GenerateRefreshToken();
        var tokenObject = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        await _refreshTokensRepo.CreateRefreshToken(tokenObject);
        return refreshToken;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task DeleteRefreshTokenAsync(int userId)
    {
        await _refreshTokensRepo.DeleteRefreshToken(userId);
    }
}
