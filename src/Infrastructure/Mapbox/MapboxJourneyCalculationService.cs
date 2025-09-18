using System.Text.Json;

public class MapboxJourneyCalculationService : IJourneyCalculationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MapboxJourneyCalculationService> _logger;
    private const string BaseUrl = "https://api.mapbox.com/directions/v5/mapbox/driving";
    private const string GeocodingBaseUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places";

    public MapboxJourneyCalculationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MapboxJourneyCalculationService> logger
    )
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MapboxRoute> CalculateRouteAsync(
        Location startLocation,
        Location endLocation,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var coordinates =
                $"{startLocation.Longitude},{startLocation.Latitude};{endLocation.Longitude},{endLocation.Latitude}";
            var accessToken =
                _configuration["MapboxAccessToken"]
                ?? throw new InvalidOperationException("MapboxAccessToken not configured");
            var url =
                @$"{BaseUrl}/{coordinates}?access_token={accessToken}&geometries=geojson&steps=true&alternatives=false
                &voice_instructions=true&voice_units=metric&banner_instructions=true&language=en&overview=full";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var mapboxResponse = JsonSerializer.Deserialize<MapboxDirectionsResponse>(
                jsonContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
            );

            if (mapboxResponse?.Routes == null || !mapboxResponse.Routes.Any())
            {
                throw new InvalidOperationException("No routes returned from Mapbox API");
            }

            var route = mapboxResponse.Routes.First();
            return route;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating route from Mapbox API");
            throw;
        }
    }

    public MapboxRoute CreateCombinedRoute(MapboxRoute driverToFoodPlace, MapboxRoute foodPlaceToCustomer)
    {
        var combinedCoordinates = driverToFoodPlace.Geometry.Coordinates
            .Concat(foodPlaceToCustomer.Geometry.Coordinates.Skip(1))
            .ToArray();

        var combinedLegs = driverToFoodPlace.Legs
            .Concat(foodPlaceToCustomer.Legs)
            .ToArray();

        return new MapboxRoute
        {
            Distance = driverToFoodPlace.Distance + foodPlaceToCustomer.Distance,
            Duration = driverToFoodPlace.Duration + foodPlaceToCustomer.Duration,
            Geometry = new MapboxGeometry
            {
                Type = "LineString",
                Coordinates = combinedCoordinates
            },
            Legs = combinedLegs
        };
    }

    public async Task<Location?> GeocodeAddressAsync(
        string address,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var accessToken =
                _configuration["MapboxAccessToken"]
                ?? throw new InvalidOperationException("MapboxAccessToken not configured");
            var encodedAddress = Uri.EscapeDataString(address);
            var url =
                $"{GeocodingBaseUrl}/{encodedAddress}.json?access_token={accessToken}&limit=1";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var geocodingResponse = JsonSerializer.Deserialize<MapboxGeocodingResponse>(
                jsonContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
            );

            if (geocodingResponse?.Features != null && geocodingResponse.Features.Any())
            {
                var feature = geocodingResponse.Features.First();
                return new Location { Longitude = feature.Center[0], Latitude = feature.Center[1] };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address: {Address}", address);
            return null;
        }
    }
}
