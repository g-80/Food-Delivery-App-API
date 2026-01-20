using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests.Helpers;

public class AuthHelper
{
    public AuthHelper() { }

    public static async Task<string> LogInUser(
        string phoneNumber,
        string password,
        HttpClient httpClient
    )
    {
        var cmd = new LogInUserCommand { PhoneNumber = phoneNumber, Password = password };

        var res = await httpClient.PostAsJsonAsync(Consts.Urls.login, cmd);
        return (await res.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
    }
}

public static class HttpClientExtensions
{
    public static HttpClient WithAuth(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
