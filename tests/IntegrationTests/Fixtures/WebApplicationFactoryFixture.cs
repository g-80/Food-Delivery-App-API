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
                services.AddScoped<IFoodPlacesRepository>(_ => new FoodPlacesRepository(
                    _connectionString,
                    _distance
                ));
                services.RemoveAll(typeof(IItemsRepository));
                services.AddScoped<IItemsRepository>(_ => new ItemsRepository(_connectionString));
                services.RemoveAll(typeof(ICartsRepository));
                services.AddScoped<ICartsRepository>(_ => new CartsRepository(_connectionString));
                services.RemoveAll(typeof(ICartItemsRepository));
                services.AddScoped<ICartItemsRepository>(_ => new CartItemsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(ICartPricingsRepository));
                services.AddScoped<ICartPricingsRepository>(_ => new CartPricingsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(IOrdersRepository));
                services.AddScoped<IOrdersRepository>(_ => new OrdersRepository(_connectionString));
                services.RemoveAll(typeof(IOrdersItemsRepository));
                services.AddScoped<IOrdersItemsRepository>(_ => new OrdersItemsRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(IUsersRepository));
                services.AddScoped<IUsersRepository>(_ => new UsersRepository(_connectionString));
                services.RemoveAll(typeof(IRefreshTokensRepository));
                services.AddScoped<IRefreshTokensRepository>(_ => new RefreshTokensRepository(
                    _connectionString
                ));
                services.RemoveAll(typeof(UnitOfWork));
                services.AddTransient(_ => new UnitOfWork(_connectionString));
                services.AddTransient<TestDataSeeder>();
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
        await seeder.SeedCartItems();

        await LoginAsACustomerAsync();
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

    public async Task LoginAsACustomerAsync()
    {
        var authService = GetServiceFromContainer<AuthService>();
        var token =
            await authService.LoginAsync(TestData.Users.loginRequests[0])
            ?? throw new Exception("User does not exist");
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public async Task LoginAsAFoodPlace()
    {
        var authService = GetServiceFromContainer<AuthService>();
        var token =
            await authService.LoginAsync(TestData.Users.loginRequests[1])
            ?? throw new Exception("User does not exist");
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
    }
}
