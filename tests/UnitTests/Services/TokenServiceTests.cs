using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

public class TokenServiceTests
{
    private readonly Mock<IUsersRepository> _mockUsersRepo;
    private readonly Mock<IRefreshTokensRepository> _mockRefreshTokensRepo;
    private readonly TokenService _tokenService;

    private const string TestJwtKey = "very_secure_key_to_use_for_jwt_tests";
    private const string TestIssuer = "test_issuer";
    private const string TestAudience = "test_audience";

    public TokenServiceTests()
    {
        _mockUsersRepo = new Mock<IUsersRepository>();
        _mockRefreshTokensRepo = new Mock<IRefreshTokensRepository>();

        var inMemorySettings = new Dictionary<string, string?> {
        {"Jwt:Key", TestJwtKey},
        {"Jwt:Issuer", TestIssuer},
        {"Jwt:Audience", TestAudience},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenService = new TokenService(_mockUsersRepo.Object, _mockRefreshTokensRepo.Object, configuration);
    }

    [Fact]
    public async Task CreateTokenResponse_ShouldReturnValidTokens()
    {
        // Arrange
        var testUser = new User { Id = 1, UserType = UserTypes.customer };
        _mockRefreshTokensRepo.Setup(r => r.CreateRefreshToken(It.IsAny<RefreshTokenDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tokenService.CreateTokenResponse(testUser);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);

        token.Issuer.Should().Be(TestIssuer);
        token.Audiences.Should().Contain(TestAudience);

        var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(testUser.Id.ToString());

        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(testUser.UserType.ToString());

        _mockRefreshTokensRepo.Verify(r => r.CreateRefreshToken(It.Is<RefreshTokenDTO>(dto =>
            dto.UserId == testUser.Id &&
            !string.IsNullOrEmpty(dto.Token) &&
            dto.ExpiresAt > DateTime.UtcNow)),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var testUser = new User { Id = 1, UserType = UserTypes.customer };
        var testRefreshToken = "valid_refresh_token";
        var refreshRequest = new RefreshTokenRequest
        {
            UserId = testUser.Id,
            RefreshToken = testRefreshToken
        };

        var savedToken = new RefreshToken
        {
            UserId = testUser.Id,
            Token = testRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockUsersRepo.Setup(r => r.GetUserById(testUser.Id))
            .ReturnsAsync(testUser);
        _mockRefreshTokensRepo.Setup(r => r.GetRefreshTokenByUserId(testUser.Id))
            .ReturnsAsync(savedToken);
        _mockRefreshTokensRepo.Setup(r => r.DeleteRefreshToken(testUser.Id))
            .Returns(Task.CompletedTask);
        _mockRefreshTokensRepo.Setup(r => r.CreateRefreshToken(It.IsAny<RefreshTokenDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tokenService.RefreshTokenAsync(refreshRequest);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(testUser.Id), Times.Once);
        _mockRefreshTokensRepo.Verify(r => r.CreateRefreshToken(It.Is<RefreshTokenDTO>(dto =>
            dto.UserId == testUser.Id &&
            !string.IsNullOrEmpty(dto.Token))),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidUserId_ShouldReturnNull()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            UserId = 999999,
            RefreshToken = "some_token"
        };

        _mockUsersRepo.Setup(r => r.GetUserById(refreshRequest.UserId))
            .ReturnsAsync((User)null);

        // Act
        var result = await _tokenService.RefreshTokenAsync(refreshRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(It.IsAny<int>()), Times.Never);
        _mockRefreshTokensRepo.Verify(r => r.CreateRefreshToken(It.IsAny<RefreshTokenDTO>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var testUser = new User { Id = 1, UserType = UserTypes.customer };
        var refreshRequest = new RefreshTokenRequest
        {
            UserId = testUser.Id,
            RefreshToken = "invalid_token"
        };

        var savedToken = new RefreshToken
        {
            UserId = testUser.Id,
            Token = "different_token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockUsersRepo.Setup(r => r.GetUserById(testUser.Id))
            .ReturnsAsync(testUser);
        _mockRefreshTokensRepo.Setup(r => r.GetRefreshTokenByUserId(testUser.Id))
            .ReturnsAsync(savedToken);

        // Act
        var result = await _tokenService.RefreshTokenAsync(refreshRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(It.IsAny<int>()), Times.Never);
        _mockRefreshTokensRepo.Verify(r => r.CreateRefreshToken(It.IsAny<RefreshTokenDTO>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var testUser = new User { Id = 1, UserType = UserTypes.customer };
        var testRefreshToken = "expired_token";
        var refreshRequest = new RefreshTokenRequest
        {
            UserId = testUser.Id,
            RefreshToken = testRefreshToken
        };

        var savedToken = new RefreshToken
        {
            UserId = testUser.Id,
            Token = testRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockUsersRepo.Setup(r => r.GetUserById(testUser.Id))
            .ReturnsAsync(testUser);
        _mockRefreshTokensRepo.Setup(r => r.GetRefreshTokenByUserId(testUser.Id))
            .ReturnsAsync(savedToken);

        // Act
        var result = await _tokenService.RefreshTokenAsync(refreshRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(It.IsAny<int>()), Times.Never);
        _mockRefreshTokensRepo.Verify(r => r.CreateRefreshToken(It.IsAny<RefreshTokenDTO>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithNonExistingToken_ShouldReturnNull()
    {
        // Arrange
        var testUser = new User { Id = 1, UserType = UserTypes.customer };
        var refreshRequest = new RefreshTokenRequest
        {
            UserId = testUser.Id,
            RefreshToken = "some_token"
        };

        _mockUsersRepo.Setup(r => r.GetUserById(testUser.Id))
            .ReturnsAsync(testUser);
        _mockRefreshTokensRepo.Setup(r => r.GetRefreshTokenByUserId(testUser.Id))
            .ReturnsAsync((RefreshToken)null);

        // Act
        var result = await _tokenService.RefreshTokenAsync(refreshRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(It.IsAny<int>()), Times.Never);
        _mockRefreshTokensRepo.Verify(r => r.CreateRefreshToken(It.IsAny<RefreshTokenDTO>()), Times.Never);
    }
}