using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.UsersTests;

public class TokenServiceTests
{
    private readonly Mock<IUserRepository> _mockUsersRepo;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokensRepo;
    private readonly TokenService _tokenService;

    private const string TestJwtKey = "very_secure_key_to_use_for_jwt_tests";
    private const string TestIssuer = "test_issuer";
    private const string TestAudience = "test_audience";
    private readonly User _testUser = new User
    {
        Id = 1,
        FirstName = "John",
        Surname = "Doe",
        Password = "Very long hash",
        PhoneNumber = "07123123123",
        UserType = UserTypes.customer,
    };

    public TokenServiceTests()
    {
        _mockUsersRepo = new Mock<IUserRepository>();
        _mockRefreshTokensRepo = new Mock<IRefreshTokenRepository>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", TestJwtKey },
            { "Jwt:Issuer", TestIssuer },
            { "Jwt:Audience", TestAudience },
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenService = new TokenService(
            _mockUsersRepo.Object,
            _mockRefreshTokensRepo.Object,
            configuration
        );
    }

    [Fact]
    public async Task CreateTokenResponse_ShouldReturnValidTokens()
    {
        // Arrange
        _mockRefreshTokensRepo
            .Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tokenService.GenerateTokens(
            _testUser.Id.ToString(),
            _testUser.UserType.ToString()
        );

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
        userIdClaim!.Value.Should().Be(_testUser.Id.ToString());

        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(_testUser.UserType.ToString());

        _mockRefreshTokensRepo.Verify(
            repo =>
                repo.AddRefreshToken(
                    It.Is<RefreshToken>(refreshToken =>
                        refreshToken.UserId == _testUser.Id
                        && !string.IsNullOrEmpty(refreshToken.Token)
                        && refreshToken.ExpiresAt > DateTime.UtcNow
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var testRefreshToken = "valid_refresh_token";
        var renewRequest = new RenewAccessTokenCommand
        {
            UserId = _testUser.Id,
            RefreshToken = testRefreshToken,
        };

        var savedToken = new RefreshToken
        {
            UserId = _testUser.Id,
            Token = testRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        _mockUsersRepo.Setup(repo => repo.GetUserById(_testUser.Id)).ReturnsAsync(_testUser);
        _mockRefreshTokensRepo
            .Setup(repo => repo.GetRefreshTokenByUserId(_testUser.Id))
            .ReturnsAsync(savedToken);
        _mockRefreshTokensRepo
            .Setup(repo => repo.DeleteRefreshToken(_testUser.Id))
            .Returns(Task.CompletedTask);
        _mockRefreshTokensRepo
            .Setup(repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tokenService.RenewAccessToken(renewRequest);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(_testUser.Id), Times.Once);
        _mockRefreshTokensRepo.Verify(
            repo =>
                repo.AddRefreshToken(
                    It.Is<RefreshToken>(refreshToken =>
                        refreshToken.UserId == _testUser.Id
                        && !string.IsNullOrEmpty(refreshToken.Token)
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshToken_WithInvalidUserId_ShouldReturnNull()
    {
        // Arrange
        var renewRequest = new RenewAccessTokenCommand
        {
            UserId = 999999,
            RefreshToken = "some_token",
        };

        _mockUsersRepo.Setup(r => r.GetUserById(renewRequest.UserId)).ReturnsAsync((User?)null);

        // Act
        var result = await _tokenService.RenewAccessToken(renewRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(r => r.DeleteRefreshToken(It.IsAny<int>()), Times.Never);
        _mockRefreshTokensRepo.Verify(
            repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var renewRequest = new RenewAccessTokenCommand
        {
            UserId = _testUser.Id,
            RefreshToken = "invalid_token",
        };

        var savedToken = new RefreshToken
        {
            UserId = _testUser.Id,
            Token = "different_token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        _mockUsersRepo.Setup(repo => repo.GetUserById(_testUser.Id)).ReturnsAsync(_testUser);
        _mockRefreshTokensRepo
            .Setup(repo => repo.GetRefreshTokenByUserId(_testUser.Id))
            .ReturnsAsync(savedToken);

        // Act
        var result = await _tokenService.RenewAccessToken(renewRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(
            repo => repo.DeleteRefreshToken(It.IsAny<int>()),
            Times.Never
        );
        _mockRefreshTokensRepo.Verify(
            repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var testRefreshToken = "expired_token";
        var renewRequest = new RenewAccessTokenCommand
        {
            UserId = _testUser.Id,
            RefreshToken = testRefreshToken,
        };

        var savedToken = new RefreshToken
        {
            UserId = _testUser.Id,
            Token = testRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
        };

        _mockUsersRepo.Setup(r => r.GetUserById(_testUser.Id)).ReturnsAsync(_testUser);
        _mockRefreshTokensRepo
            .Setup(r => r.GetRefreshTokenByUserId(_testUser.Id))
            .ReturnsAsync(savedToken);

        // Act
        var result = await _tokenService.RenewAccessToken(renewRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(
            repo => repo.DeleteRefreshToken(It.IsAny<int>()),
            Times.Never
        );
        _mockRefreshTokensRepo.Verify(
            repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshToken_WithNonExistingToken_ShouldReturnNull()
    {
        // Arrange
        var renewRequest = new RenewAccessTokenCommand
        {
            UserId = _testUser.Id,
            RefreshToken = "some_token",
        };

        _mockUsersRepo.Setup(repo => repo.GetUserById(_testUser.Id)).ReturnsAsync(_testUser);
        _mockRefreshTokensRepo
            .Setup(repo => repo.GetRefreshTokenByUserId(_testUser.Id))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _tokenService.RenewAccessToken(renewRequest);

        // Assert
        result.Should().BeNull();

        _mockRefreshTokensRepo.Verify(
            repo => repo.DeleteRefreshToken(It.IsAny<int>()),
            Times.Never
        );
        _mockRefreshTokensRepo.Verify(
            repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never
        );
    }
}
