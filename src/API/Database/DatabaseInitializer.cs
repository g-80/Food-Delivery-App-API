using DbUp;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void InitializeDatabase(bool includeSeedData = true)
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);
        var builder = DeployChanges.To.PostgresqlDatabase(_connectionString).LogToConsole();
        builder = builder.WithScriptsEmbeddedInAssembly(
            typeof(DatabaseInitializer).Assembly,
            script => script.Contains("Database.Scripts.Migrations")
        );

        if (includeSeedData)
        {
            builder = builder.WithScriptsEmbeddedInAssembly(
                typeof(DatabaseInitializer).Assembly,
                script => script.Contains("Database.Scripts.Seeds")
            );
        }

        var upgrader = builder.Build();

        if (upgrader.IsUpgradeRequired())
        {
            upgrader.PerformUpgrade();
        }
    }
}
