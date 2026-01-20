using StackExchange.Redis;

namespace IntegrationTests.Helpers;

public class RedisHelper
{
    private readonly string _connectionString;
    private readonly Lazy<ConnectionMultiplexer> _connection;
    private IDatabase Database => _connection.Value.GetDatabase();

    public RedisHelper(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new Lazy<ConnectionMultiplexer>(
            () => ConnectionMultiplexer.Connect(_connectionString + ",allowAdmin=true")
        );
    }

    public async Task SeedDriverLocation(int driverId, double latitude, double longitude)
    {
        var db = Database;
        var driverIdStr = driverId.ToString();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await db.GeoAddAsync("drivers:locations", longitude, latitude, driverIdStr);
        await db.HashSetAsync("drivers:locations:timestamps", driverIdStr, timestamp);
        await db.StringSetAsync($"drivers:statuses:{driverId}", 1); // 1 = DriverStatuses.online
    }

    public async Task<Location?> GetDriverLocation(int driverId)
    {
        var db = Database;
        var geoPos = await db.GeoPositionAsync("drivers:locations", driverId.ToString());

        if (!geoPos.HasValue)
            return null;

        return new Location
        {
            Latitude = geoPos.Value.Latitude,
            Longitude = geoPos.Value.Longitude,
        };
    }

    public async Task<string?> GetDriverStatus(int driverId)
    {
        return await Database.StringGetAsync($"drivers:statuses:{driverId}");
    }

    public async Task<long?> GetDriverLocationTimestamp(int driverId)
    {
        var timestamp = await Database.HashGetAsync(
            "drivers:locations:timestamps",
            driverId.ToString()
        );
        return timestamp.HasValue ? long.Parse(timestamp!) : null;
    }

    public async Task FlushDb()
    {
        var server = _connection.Value.GetServer(_connection.Value.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }
    }
}
