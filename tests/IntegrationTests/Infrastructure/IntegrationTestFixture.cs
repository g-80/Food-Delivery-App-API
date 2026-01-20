using DotNet.Testcontainers.Builders;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests.Infrastructure;

public class IntegrationTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;

    public string PostgresConnectionString { get; private set; } = string.Empty;
    public string RedisConnectionString { get; private set; } = string.Empty;

    public Respawner? Respawner { get; private set; }

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:17-3.5")
            .WithDatabase("fooddeliveryapp_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilCommandIsCompleted("pg_isready")
                    .UntilExternalTcpPortIsAvailable(5432)
                    .UntilDatabaseIsAvailable(NpgsqlFactory.Instance)
            )
            .Build();

        await _postgresContainer.StartAsync();
        PostgresConnectionString = _postgresContainer.GetConnectionString();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:8")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("redis-cli", "ping"))
            .Build();

        await _redisContainer.StartAsync();
        RedisConnectionString = _redisContainer.GetConnectionString();

        await RunMigrations();

        using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();
        Respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { TablesToIgnore = ["spatial_ref_sys"] }
        );
    }

    private async Task RunMigrations()
    {
        var options = new Microsoft.Extensions.Options.OptionsWrapper<DatabaseOptions>(
            new DatabaseOptions { ConnectionString = PostgresConnectionString }
        );

        var initialiser = new DatabaseInitialiser(options);
        initialiser.InitialiseDatabase(false);

        // Wait for migrations to complete
        await Task.Delay(500);
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }

        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }
}
