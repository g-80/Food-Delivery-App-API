using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

public class WebApplicationFactoryFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    public HttpClient Client { get; private set; }
    private string _connectionString = string.Empty;
    private int _distance;
    private DatabaseInitializer _dbInitializer;

    public WebApplicationFactoryFixture()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(
                (_, configBuilder) =>
                {
                    var config = configBuilder.Build();
                    _connectionString =
                        config.GetConnectionString("Tests")
                        ?? throw new Exception("Could not read the connectionstring");
                    _distance = config.GetValue<int>("SearchDistance:Default");
                }
            );
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(IFoodPlacesRepository));
                services.AddSingleton<IFoodPlacesRepository>(_ => new FoodPlacesRepository(
                    _connectionString,
                    _distance
                ));
                services.RemoveAll(typeof(IItemsRepository));
                services.AddSingleton<IItemsRepository>(_ => new ItemsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(ICartsRepository));
                services.AddSingleton<ICartsRepository>(_ => new CartsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(ICartItemsRepository));
                services.AddSingleton<ICartItemsRepository>(_ => new CartItemsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(ICartPricingsRepository));
                services.AddSingleton<ICartPricingsRepository>(_ => new CartPricingsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(IOrdersRepository));
                services.AddSingleton<IOrdersRepository>(_ => new OrdersRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(IOrdersItemsRepository));
                services.AddSingleton<IOrdersItemsRepository>(_ => new OrdersItemsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(IUsersRepository));
                services.AddTransient<IUsersRepository>(_ => new UsersRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(IRefreshTokensRepository));
                services.AddTransient<IRefreshTokensRepository>(_ => new RefreshTokensRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(UnitOfWork));
                services.AddTransient(_ => new UnitOfWork(_connectionString));
                services.AddSingleton<TestDataSeeder>();
                services.AddSingleton<LoginHelper>();
            });
        });
        Client = _factory.CreateClient();
        _dbInitializer = new DatabaseInitializer(_connectionString, false);
    }

    public async Task InitializeAsync()
    {
        _dbInitializer.InitializeDatabase();

        var seeder = GetServiceFromContainer<TestDataSeeder>();
        await seeder.SeedFoodPlaces();
        await seeder.SeedItems();
        await seeder.SeedUsers();
        await seeder.SeedCartData();

        var loginHelper = GetServiceFromContainer<LoginHelper>();
        await loginHelper.LoginAsACustomer();
        await loginHelper.LoginAsAFoodPlace();
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(
            _connectionString
        );
        string DbName =
            builder.Database
            ?? throw new Exception("Could not read the database name from the connectionstring");
        builder.Database = "postgres";
        using (var connection = new NpgsqlConnection(builder.ConnectionString))
        {
            await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {DbName} WITH (FORCE)");
        }
        ;
    }

    public T GetServiceFromContainer<T>()
        where T : notnull
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        return service;
    }

    public void SetCustomerAccessToken()
    {
        var loginHelper = GetServiceFromContainer<LoginHelper>();
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginHelper._customerAccessToken
            );
    }

    public void SetFoodPlaceAccessToken()
    {
        var loginHelper = GetServiceFromContainer<LoginHelper>();
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginHelper._foodPlaceAccessToken
            );
    }
}
