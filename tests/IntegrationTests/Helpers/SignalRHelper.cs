using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace IntegrationTests.Helpers;

public class SignalRHelper
{
    private readonly CustomWebApplicationFactory _factory;
    public SignalRHelper(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task<HubConnection> CreateDriverConnection(string baseUrl, string token)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/driver?access_token={token}", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        // need the delay because onConnectedAsync starts running
        // after StartAsync completes and the connection returns
        await Task.Delay(250);
        return connection;
    }

    public async Task<HubConnection> CreateFoodPlaceConnection(string baseUrl, string token)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/foodplace?access_token={token}", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        return connection;
    }
}
