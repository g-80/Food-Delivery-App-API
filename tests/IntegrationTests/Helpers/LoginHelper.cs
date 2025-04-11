public class LoginHelper
{
    private readonly AuthService _authService;
    public string _customerAccessToken { get; private set; } = string.Empty;
    public string _foodPlaceAccessToken { get; private set; } = string.Empty;
    public LoginHelper(AuthService authService)
    {
        _authService = authService;
    }

    public async Task LoginAsACustomer()
    {
        var token = await _authService.LoginAsync(TestData.Users.loginRequests[0]) ?? throw new Exception("User does not exist");
        _customerAccessToken = token.AccessToken;
    }

    public async Task LoginAsAFoodPlace()
    {
        var token = await _authService.LoginAsync(TestData.Users.loginRequests[1]) ?? throw new Exception("User does not exist");
        _foodPlaceAccessToken = token.AccessToken;
    }
}