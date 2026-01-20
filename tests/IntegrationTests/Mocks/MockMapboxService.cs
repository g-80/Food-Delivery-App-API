namespace IntegrationTests.Mocks;

public class MockMapboxService : IJourneyCalculationService
{
    private const double FIXED_DISTANCE = 1000.0; // 1km for all routes
    private const double FIXED_DURATION = 600.0; // 10 minutes for all routes
    private const double FIXED_LEG_DISTANCE = 500.0; // 500m per leg

    public Task<(MapboxRouteInfo, string)> CalculateRouteAsync(
        Location[] locations,
        CancellationToken cancellationToken = default
    )
    {
        var legs = new List<MapboxLegInfo>();
        var legCount = locations.Length - 1;

        for (int i = 0; i < legCount; i++)
        {
            legs.Add(
                new MapboxLegInfo
                {
                    Distance = FIXED_LEG_DISTANCE,
                    Duration = FIXED_DURATION / legCount,
                }
            );
        }

        var routeInfo = new MapboxRouteInfo
        {
            Distance = FIXED_DISTANCE,
            Duration = FIXED_DURATION,
            Legs = legs.ToArray(),
        };

        var json = System.Text.Json.JsonSerializer.Serialize(new { routes = new[] { routeInfo } });

        return Task.FromResult((routeInfo, json));
    }

    public Task<Location?> GeocodeAddressAsync(
        string address,
        CancellationToken cancellationToken = default
    )
    {
        var location = new Location { Latitude = 51.5074, Longitude = -0.1278 };

        return Task.FromResult<Location?>(location);
    }
}
