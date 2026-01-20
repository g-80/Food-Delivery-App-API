using IntegrationTests.Helpers;
using Npgsql;

namespace IntegrationTests.Infrastructure;

[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IntegrationTestFixture Fixture;
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly AuthHelper AuthHelper;
    protected readonly SignalRHelper SignalRHelper;
    protected readonly RedisHelper RedisHelper;
    protected readonly TestDataBuilder TestDataBuilder;
    protected readonly HttpClient Client;
    protected readonly string BaseUrl;

    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
        Factory = new CustomWebApplicationFactory(fixture);
        Client = Factory.CreateClient();
        BaseUrl = Client.BaseAddress!.ToString().TrimEnd('/');

        AuthHelper = new AuthHelper();
        SignalRHelper = new SignalRHelper(Factory);
        RedisHelper = new RedisHelper(fixture.RedisConnectionString);
        TestDataBuilder = new TestDataBuilder(Client);
    }

    public virtual async Task InitializeAsync()
    {
        await TestDataBuilder.SeedMinimalTestData();

        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        using var connection = new NpgsqlConnection(Fixture.PostgresConnectionString);
        await connection.OpenAsync();
        await Fixture.Respawner!.ResetAsync(connection);

        await RedisHelper.FlushDb();
        RedisHelper.Dispose();

        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
