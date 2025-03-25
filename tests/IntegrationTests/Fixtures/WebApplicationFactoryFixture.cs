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
    private DatabaseInitializer _dbInitializer;
    public TestDataSeeder _seeder;

    public WebApplicationFactoryFixture()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                var config = configBuilder.Build();
                _connectionString = config.GetConnectionString("Tests") ?? throw new Exception("Could not read the connectionstring");
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(FoodPlacesRepository));
                services.AddTransient(_ => new FoodPlacesRepository(_connectionString));
                services.RemoveAll(typeof(ItemsRepository));
                services.AddTransient(_ => new ItemsRepository(_connectionString));
                services.RemoveAll(typeof(CartsRepository));
                services.AddTransient(_ => new CartsRepository(_connectionString));
                services.RemoveAll(typeof(CartItemsRepository));
                services.AddTransient(_ => new CartItemsRepository(_connectionString));
                services.RemoveAll(typeof(CartPricingsRepository));
                services.AddTransient(_ => new CartPricingsRepository(_connectionString));
                services.RemoveAll(typeof(OrdersRepository));
                services.AddTransient(_ => new OrdersRepository(_connectionString));
                services.RemoveAll(typeof(OrdersItemsRepository));
                services.AddTransient(_ => new OrdersItemsRepository(_connectionString));
                services.RemoveAll(typeof(UnitOfWork));
                services.AddTransient(_ => new UnitOfWork(_connectionString));
                services.AddSingleton<TestDataSeeder>();
            });
        });
        Client = _factory.CreateClient();
        _dbInitializer = new DatabaseInitializer(_connectionString, false);
        _seeder = GetServiceFromContainer<TestDataSeeder>();
    }

    public async Task InitializeAsync()
    {
        await _dbInitializer.InitializeAsync();
        await _seeder.SeedFoodPlaces();
        await _seeder.SeedItems();
        await _seeder.SeedCartData();
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(_connectionString);
        string DbName = builder.Database ?? throw new Exception("Could not read the database name from the connectionstring");
        builder.Database = "postgres";
        using (var connection = new NpgsqlConnection(builder.ConnectionString))
        {
            await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {DbName} WITH (FORCE)");
        }
        ;
    }

    public T GetServiceFromContainer<T>() where T : notnull
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        return service;
    }
}