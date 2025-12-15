using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using NRedisStack;
using StackExchange.Redis;

public class DriverRepository : BaseRepository, IDriverRepository
{
    private const string GeoKey = "drivers:locations";
    private const string TimestampsKey = "drivers:locations:timestamps";
    private const string StatusKeyPrefix = "drivers:statuses:";
    private const int StalenessPeriodSeconds = 120;
    private readonly RedisConnectionFactory _redisConnectionFactory;

    public DriverRepository(
        IOptions<DatabaseOptions> options,
        RedisConnectionFactory redisConnectionFactory
    )
        : base(options.Value.ConnectionString)
    {
        _redisConnectionFactory = redisConnectionFactory;
    }

    public async Task<bool> ConnectDriver(
        int driverId,
        DriverStatuses status = DriverStatuses.online
    )
    {
        var key = $"{StatusKeyPrefix}{driverId}";
        var db = _redisConnectionFactory.GetDatabase();
        return await db.StringSetAsync(key, (int)status);
    }

    public async Task DisconnectDriver(int driverId)
    {
        var statusKey = $"{StatusKeyPrefix}{driverId}";
        var driverIdStr = driverId.ToString();

        var db = _redisConnectionFactory.GetDatabase();
        var tran = new Transaction(db);
        _ = tran.Db.KeyDeleteAsync(statusKey);
        _ = tran.Db.HashDeleteAsync(TimestampsKey, driverIdStr);
        _ = tran.Db.GeoRemoveAsync(GeoKey, driverIdStr);

        await tran.ExecuteAsync();
    }

    public async Task<IEnumerable<AvailableDriver>> GetAvailableDriversWithinDistance(
        double latitude,
        double longitude,
        int distance,
        DriverStatuses status = DriverStatuses.online
    )
    {
        var db = _redisConnectionFactory.GetDatabase();
        var searchResult = await db.GeoSearchAsync(
            GeoKey,
            longitude,
            latitude,
            new GeoSearchCircle(distance, GeoUnit.Meters),
            10,
            order: StackExchange.Redis.Order.Ascending,
            options: GeoRadiusOptions.WithDistance | GeoRadiusOptions.WithCoordinates
        );

        if (searchResult.Length == 0)
            return Enumerable.Empty<AvailableDriver>();

        var batch = db.CreateBatch();
        var driverIds = searchResult.Select(r => r.Member.ToString()).ToArray();

        var statusTasks = driverIds
            .Select(id => batch.StringGetAsync($"{StatusKeyPrefix}{id}"))
            .ToArray();

        var timestampTask = batch.HashGetAsync(
            TimestampsKey,
            driverIds.Select(id => (RedisValue)id).ToArray()
        );

        batch.Execute();

        var statuses = await Task.WhenAll(statusTasks);
        var timestamps = await timestampTask;

        // filter and build results
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var stalenessCutoff = currentTime - StalenessPeriodSeconds;
        var availableDrivers = new List<AvailableDriver>();

        for (int i = 0; i < searchResult.Length; i++)
        {
            var driverId = int.Parse(driverIds[i]);
            var driverStatus = statuses[i];
            var timestampStr = timestamps[i];

            if ((int)driverStatus != (int)status)
                continue;

            var timestamp = long.Parse(timestampStr!);
            if (timestamp < stalenessCutoff)
                continue;

            availableDrivers.Add(
                new AvailableDriver
                {
                    Id = driverId,
                    Status = (DriverStatuses)(int)driverStatus,
                    Location = new Location
                    {
                        Latitude = searchResult[i].Position!.Value.Latitude,
                        Longitude = searchResult[i].Position!.Value.Longitude,
                    },
                    Distance = searchResult[i].Distance ?? 0, // its possible that an available driver is at the exact same coordinates of the food place
                }
            );
        }

        return availableDrivers;
    }

    public async Task<Driver?> GetDriverById(int id)
    {
        var statusKey = $"{StatusKeyPrefix}{id}";

        var db = _redisConnectionFactory.GetDatabase();
        var statusStr = await db.StringGetAsync(statusKey);

        if (statusStr.IsNullOrEmpty)
            return null;

        var driver = new Driver { Id = id, Status = (DriverStatuses)(int)statusStr };

        var geoPos = await db.GeoPositionAsync(GeoKey, id.ToString());
        if (geoPos.HasValue)
        {
            driver.Location = new Location
            {
                Latitude = geoPos.Value.Latitude,
                Longitude = geoPos.Value.Longitude,
            };
        }

        return driver;
    }

    public async Task UpdateDriverStatus(Driver driver)
    {
        var key = $"{StatusKeyPrefix}{driver.Id}";
        var db = _redisConnectionFactory.GetDatabase();
        await db.StringSetAsync(key, (int)driver.Status);
    }

    public async Task UpsertDriverLocation(Driver driver)
    {
        if (driver.Location == null)
            throw new ArgumentNullException(nameof(driver.Location));

        var db = _redisConnectionFactory.GetDatabase();
        var transaction = db.CreateTransaction();

        var timestampMs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var driverId = driver.Id.ToString();

        _ = transaction.GeoAddAsync(
            GeoKey,
            driver.Location.Longitude,
            driver.Location.Latitude,
            driverId
        );

        _ = transaction.HashSetAsync(TimestampsKey, driverId, timestampMs);

        await transaction.ExecuteAsync();
    }

    public async Task AddDriverLocationHistoryAsync(DriverLocationHistory locationHistory)
    {
        const string sql =
            @"
            INSERT INTO driver_location_history
            (driver_id, latitude, longitude, accuracy, speed, heading, timestamp, delivery_id)
            VALUES (@DriverId, @Latitude, @Longitude, @Accuracy, @Speed, @Heading, @Timestamp, @DeliveryId)
            ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            sql,
            new
            {
                locationHistory.DriverId,
                locationHistory.Location.Latitude,
                locationHistory.Location.Longitude,
                locationHistory.Accuracy,
                locationHistory.Speed,
                locationHistory.Heading,
                locationHistory.Timestamp,
                locationHistory.DeliveryId,
            }
        );
    }
}
