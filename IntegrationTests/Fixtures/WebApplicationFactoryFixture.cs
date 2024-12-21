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
            });
        });
        Client = _factory.CreateClient();
        _dbInitializer = new DatabaseInitializer(_connectionString);
    }

    public async Task InitializeAsync()
    {
        await _dbInitializer.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(_connectionString);
        string DbName = builder.Database ?? throw new Exception("Could not read the database name from the connectionstring");
        builder.Database = "postgres";
        using (var connection = new NpgsqlConnection(builder.ConnectionString))
        {
            connection.Execute($"DROP DATABASE IF EXISTS {DbName} WITH (FORCE)");
        };
    }
}