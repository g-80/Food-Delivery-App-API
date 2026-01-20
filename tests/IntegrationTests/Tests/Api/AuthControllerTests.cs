using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Tests.Api;

public class AuthControllerTests : IntegrationTestBase
{
    private readonly DatabaseHelper _databaseHelper;

    public AuthControllerTests(IntegrationTestFixture fixture)
        : base(fixture)
    {
        _databaseHelper = new DatabaseHelper(fixture.PostgresConnectionString);
    }

    [Fact]
    public async Task SignUp_ValidRequest_CreatesUserAndReturns200()
    {
        // Arrange
        var command = new
        {
            FirstName = "John",
            Surname = "Doe",
            PhoneNumber = "07999999999",
            Password = "Password123!",
            UserType = 1, // customer
            Address = new
            {
                NumberAndStreet = Consts.Addresses.Street,
                City = Consts.Addresses.City,
                Postcode = Consts.Addresses.Postcode,
            },
        };

        // Act
        var response = await Client.PostAsJsonAsync(Consts.Urls.signup, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await _databaseHelper.GetUserByPhoneNumber(command.PhoneNumber);
        user.Should().NotBeNull();
        user.FirstName.Should().Be("John");
        user.Surname.Should().Be("Doe");

        var cart = await _databaseHelper.GetCartByCustomerId(user.Id);
        cart.Should().NotBeNull();
        cart.CustomerId.Should().Be(user.Id);
    }

    [Fact]
    public async Task SignUp_DuplicatePhoneNumber_Returns400()
    {
        // Arrange
        var phoneNumber = "07456456456";
        var command1 = new
        {
            FirstName = "First",
            Surname = "User",
            PhoneNumber = phoneNumber,
            Password = "Password123!",
            UserType = 1,
            Address = new
            {
                NumberAndStreet = Consts.Addresses.Street,
                City = Consts.Addresses.City,
                Postcode = Consts.Addresses.Postcode,
            },
        };
        var response1 = await Client.PostAsJsonAsync(Consts.Urls.signup, command1);
        response1.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);

        var command2 = new
        {
            FirstName = "Duplicate",
            Surname = "User",
            PhoneNumber = phoneNumber,
            Password = "Password123!",
            UserType = 1,
            Address = new
            {
                NumberAndStreet = Consts.Addresses.Street,
                City = Consts.Addresses.City,
                Postcode = Consts.Addresses.Postcode,
            },
        };

        // Act
        var response2 = await Client.PostAsJsonAsync(Consts.Urls.signup, command2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await _databaseHelper.GetUserByPhoneNumber(phoneNumber);
        user.Should().NotBeNull();
        user.FirstName.Should().Be("First");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        // Arrange

        var loginCmd = new
        {
            PhoneNumber = Consts.CustomerPhoneNumber,
            Password = Consts.TestPassword,
        };

        // Act
        var response = await Client.PostAsJsonAsync(Consts.Urls.login, loginCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns400()
    {
        // Arrange
        // no user with this phone number
        var loginCmd = new { PhoneNumber = "07147147147", Password = "WrongPassword123!" };

        // Act
        var response = await Client.PostAsJsonAsync(Consts.Urls.login, loginCmd);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
