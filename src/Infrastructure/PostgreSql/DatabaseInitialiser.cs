using DbUp;
using Microsoft.Extensions.Options;

public class DatabaseInitialiser : IDatabaseInitialiser
{
    private readonly string _connectionString;

    public DatabaseInitialiser(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public void InitialiseDatabase(bool includeSeedData = true)
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);
        var builder = DeployChanges.To.PostgresqlDatabase(_connectionString).LogToConsole();
        builder = builder.WithScriptsEmbeddedInAssembly(
            typeof(DatabaseInitialiser).Assembly,
            script => script.Contains("Scripts.Migrations")
        );

        if (includeSeedData)
        {
            builder = builder.WithScriptsEmbeddedInAssembly(
                typeof(DatabaseInitialiser).Assembly,
                script => script.Contains("Scripts.Seeds")
            );
        }

        var upgrader = builder.Build();

        if (upgrader.IsUpgradeRequired())
        {
            upgrader.PerformUpgrade();
        }
    }
}
