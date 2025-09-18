public interface IJourneyCalculationService
{
    Task<MapboxRoute> CalculateRouteAsync(
        Location startLocation,
        Location endLocation,
        CancellationToken cancellationToken = default
    );

    Task<Location?> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default);

    MapboxRoute CreateCombinedRoute(MapboxRoute driverToFoodPlace, MapboxRoute foodPlaceToCustomer);
}