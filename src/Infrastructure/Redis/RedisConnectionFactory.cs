using Microsoft.Extensions.Options;
using StackExchange.Redis;

public class RedisConnectionFactory
{
    private readonly Lazy<ConnectionMultiplexer> _connection;

    public RedisConnectionFactory(IOptions<RedisOptions> options)
    {
        _connection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(options.Value.ConnectionString);
        });
    }

    public IConnectionMultiplexer Connection => _connection.Value;

    public IDatabase GetDatabase() => Connection.GetDatabase();
}
