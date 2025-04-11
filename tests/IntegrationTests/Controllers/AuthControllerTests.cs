using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Routing;

[Collection("Controllers collection")]
public class AuthControllerTests
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly IUsersRepository _usersRepo;
    private readonly IRefreshTokensRepository _refreshTokensRepo;

    public AuthControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _usersRepo = _factory.GetServiceFromContainer<IUsersRepository>();
        _refreshTokensRepo = _factory.GetServiceFromContainer<IRefreshTokensRepository>();
    }

    [Fact]
    public async Task SignUp_WithValidData_CreatesUserInRepository()
    {
        // Arrange
        var signUpRequest = new CreateUserRequest
        {
            FirstName = "John",
            Surname = "Doe",
            PhoneNumber = "07111222333",
            Password = "SecurePassword!",
            UserType = UserTypes.customer
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.SignUp, signUpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await _usersRepo.GetUserByPhoneNumber(signUpRequest.PhoneNumber);

        user.Should().NotBeNull();
        user!.FirstName.Should().Be(signUpRequest.FirstName);
        user.Surname.Should().Be(signUpRequest.Surname);
        user.PhoneNumber.Should().Be(signUpRequest.PhoneNumber);
        user.UserType.Should().Be(signUpRequest.UserType);
        user.Password.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SignUp_WithExistingPhoneNumber_ReturnsBadRequest()
    {
        // Arrange
        var signUpRequest = new CreateUserRequest
        {
            FirstName = "Jane",
            Surname = "Smith",
            PhoneNumber = "07333666999",
            Password = "SecurePassword!",
            UserType = UserTypes.customer
        };

        // Act
        await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.SignUp, signUpRequest);

        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.SignUp, signUpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await _usersRepo.GetUserByPhoneNumber(signUpRequest.PhoneNumber);

        user.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithValidCredentials_CreatesRefreshToken()
    {
        // Arrange
        var signUpRequest = new CreateUserRequest
        {
            FirstName = "Test",
            Surname = "User",
            PhoneNumber = "07555666333",
            Password = "SecurePassword!",
            UserType = UserTypes.customer
        };
        await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.SignUp, signUpRequest);

        var loginRequest = new UserLoginRequest
        {
            PhoneNumber = signUpRequest.PhoneNumber,
            Password = signUpRequest.Password
        };

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Login, loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
        tokenResponse.RefreshToken.Should().NotBeNullOrEmpty();


        var user = await _usersRepo.GetUserByPhoneNumber(loginRequest.PhoneNumber);
        user.Should().NotBeNull();

        var refreshToken = await _refreshTokensRepo.GetRefreshTokenByUserId(user!.Id);
        refreshToken.Should().NotBeNull();
        refreshToken!.Token.Should().Be(tokenResponse.RefreshToken);
        refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = TestData.Users.loginRequests[1];
        loginRequest.Password = "WrongPassword123!";

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Login, loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReplacesRefreshToken()
    {
        // Arrange
        var signUpRequest = new CreateUserRequest
        {
            FirstName = "Refresh",
            Surname = "The Token",
            PhoneNumber = "07111444777",
            Password = "SecurePassword!",
            UserType = UserTypes.customer
        };
        await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.SignUp, signUpRequest);

        var loginRequest = new UserLoginRequest
        {
            PhoneNumber = signUpRequest.PhoneNumber,
            Password = signUpRequest.Password
        };

        var loginResponse = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Login, loginRequest);
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenResponse!.AccessToken);
        var userId = int.Parse(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        string originalRefreshToken = tokenResponse!.RefreshToken;

        var refreshRequest = new RefreshTokenRequest
        {
            UserId = userId,
            RefreshToken = originalRefreshToken
        };

        _factory.SetCustomerAccessToken();

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.RefreshToken, refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        newTokenResponse.Should().NotBeNull();
        newTokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
        newTokenResponse.RefreshToken.Should().NotBeNullOrEmpty();

        newTokenResponse.RefreshToken.Should().NotBe(tokenResponse.RefreshToken);

        var refreshToken = await _refreshTokensRepo.GetRefreshTokenByUserId(userId);
        refreshToken.Should().NotBeNull();
        refreshToken!.Token.Should().Be(newTokenResponse.RefreshToken);
        refreshToken.Token.Should().NotBe(originalRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var signUpRequest = new CreateUserRequest
        {
            FirstName = "Invalid",
            Surname = "Refresh",
            PhoneNumber = "07888555222",
            Password = "SecurePassword!",
            UserType = UserTypes.customer
        };
        await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.SignUp, signUpRequest);

        var loginRequest = new UserLoginRequest
        {
            PhoneNumber = signUpRequest.PhoneNumber,
            Password = signUpRequest.Password
        };

        var loginResponse = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.Login, loginRequest);
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenResponse!.AccessToken);
        var userId = int.Parse(jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        string originalRefreshToken = tokenResponse!.RefreshToken;

        var refreshRequest = new RefreshTokenRequest
        {
            UserId = userId,
            RefreshToken = "invalid-refresh-token"
        };

        _factory.SetCustomerAccessToken();

        // Act
        var response = await _factory.Client.PostAsJsonAsync(HttpHelper.Urls.RefreshToken, refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var refreshToken = await _refreshTokensRepo.GetRefreshTokenByUserId(userId);
        refreshToken.Should().NotBeNull();
        refreshToken!.Token.Should().Be(originalRefreshToken);
    }
}
