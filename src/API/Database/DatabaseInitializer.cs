using DbUp;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly bool _includeSeedData;

    public DatabaseInitializer(string connectionString, bool includeSeedData = true)
    {
        _connectionString = connectionString;
        _includeSeedData = includeSeedData;
    }

    public void InitializeDatabase()
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);
        var builder = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .LogToConsole();
        builder = builder.WithScriptsEmbeddedInAssembly(
            typeof(DatabaseInitializer).Assembly,
            script => script.Contains("Database.Scripts.Migrations"));

        // Only include seed scripts if specified
        if (_includeSeedData)
        {
            builder = builder.WithScriptsEmbeddedInAssembly(
                typeof(DatabaseInitializer).Assembly,
                script => script.Contains("Database.Scripts.Seeds"));
        }

        var upgrader = builder.Build();

        if (upgrader.IsUpgradeRequired())
        {
            upgrader.PerformUpgrade();
        }
    }
}