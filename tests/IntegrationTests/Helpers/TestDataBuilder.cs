using System.Net.Http.Json;

namespace IntegrationTests.Helpers;

public class TestDataBuilder
{
    private readonly HttpClient _httpClient;

    public TestDataBuilder(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public AddressCreateRequest addressReq = new()
    {
        NumberAndStreet = Consts.Addresses.Street,
        City = Consts.Addresses.City,
        Postcode = Consts.Addresses.Postcode,
    };

    public async Task CreateTestUser(
        string phoneNumber,
        string firstName,
        string surname,
        UserTypes userType
    )
    {
        var command = new SignUpUserCommand
        {
            FirstName = firstName,
            Surname = surname,
            PhoneNumber = phoneNumber,
            Password = Consts.TestPassword,
            UserType = userType,
            Address = addressReq,
        };

        await _httpClient.PostAsJsonAsync(Consts.Urls.signup, command);
    }

    public async Task CreateTestFoodPlace(string phoneNumber)
    {
        var token = await AuthHelper.LogInUser(phoneNumber, Consts.TestPassword, _httpClient);

        var command = new CreateFoodPlaceCommand
        {
            Name = "Test Restaurant",
            Description = "A test food place",
            Category = "Italian",
            Latitude = Consts.LondonCentralLat,
            Longitude = Consts.LondonCentralLong,
            Address = new AddressCreateRequest
            {
                NumberAndStreet = "456 Restaurant Ave",
                City = "London",
                Postcode = "W1A 1AA",
            },
        };

        await _httpClient.WithAuth(token).PostAsJsonAsync(Consts.Urls.foodPlaces, command);
    }

    public async Task CreateTestFoodPlaceItem(string phoneNumber)
    {
        var token = await AuthHelper.LogInUser(phoneNumber, Consts.TestPassword, _httpClient);

        var command = new CreateItemCommand
        {
            Name = "Test Pizza",
            Description = "A delicious test pizza",
            Price = Consts.Prices.DefaultItemPrice,
            IsAvailable = true,
        };

        await _httpClient.WithAuth(token).PostAsJsonAsync(Consts.Urls.foodPlacesItems, command);
    }

    public async Task SeedMinimalTestData()
    {
        await CreateTestUser(Consts.CustomerPhoneNumber, "The", "Customer", UserTypes.customer);
        await CreateTestUser(
            Consts.FoodPlacePhoneNumber,
            "The",
            "Food place",
            UserTypes.food_place
        );
        await CreateTestUser(Consts.DriverPhoneNumber, "The", "Driver", UserTypes.driver);

        await CreateTestFoodPlace(Consts.FoodPlacePhoneNumber);
        await CreateTestFoodPlaceItem(Consts.FoodPlacePhoneNumber);
    }

    public async Task AddTestCartItem(string phoneNumber, int itemId, int quantity = 1)
    {
        var token = await AuthHelper.LogInUser(phoneNumber, Consts.TestPassword, _httpClient);

        var command = new AddItemCommand { ItemId = itemId, Quantity = quantity };

        await _httpClient.WithAuth(token).PostAsJsonAsync(Consts.Urls.carts, command);
    }

    // change the cart item and food place method to sql and move to databasehelper
}
