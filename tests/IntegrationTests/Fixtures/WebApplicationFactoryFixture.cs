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
    private string _connectionString;
    private DatabaseInitializer _dbInitializer;

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
                services.RemoveAll(typeof(QuotesRepository));
                services.AddTransient(_ => new QuotesRepository(_connectionString));
                services.RemoveAll(typeof(QuotesItemsRepository));
                services.AddTransient(_ => new QuotesItemsRepository(_connectionString));
                services.RemoveAll(typeof(OrdersRepository));
                services.AddTransient(_ => new OrdersRepository(_connectionString));
                services.RemoveAll(typeof(OrderItemsRepository));
                services.AddTransient(_ => new OrderItemsRepository(_connectionString));
            });
        });
        Client = _factory.CreateClient();
        _dbInitializer = new DatabaseInitializer(_connectionString, false);
    }

    public async Task InitializeAsync()
    {
        await _dbInitializer.InitializeAsync();
        var foodPlaceRepo = GetRepoFromServices<FoodPlacesRepository>();
        foreach (var foodPlace in Fixtures.foodPlacesFixtures)
        {
            await foodPlaceRepo.CreateFoodPlace(foodPlace);
        }
        var itemRepo = GetRepoFromServices<ItemsRepository>();
        foreach (var item in Fixtures.itemsFixtures)
        {
            Fixtures.itemsFixturesIds.Add(await itemRepo.CreateItem(item));
        }
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(_connectionString);
        string DbName = builder.Database ?? throw new Exception("Could not read the database name from the connectionstring");
        builder.Database = "postgres";
        using (var connection = new NpgsqlConnection(builder.ConnectionString))
        {
            connection.Execute($"DROP DATABASE IF EXISTS {DbName} WITH (FORCE)");
        }
        ;
    }

    public T GetRepoFromServices<T>() where T : notnull
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<T>();
        return repository;
    }
}