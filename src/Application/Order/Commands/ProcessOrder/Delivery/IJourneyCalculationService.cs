public interface IJourneyCalculationService
{
    Task<(MapboxRouteInfo, string)> CalculateRouteAsync(
        Location[] locations,
        CancellationToken cancellationToken = default
    );

    Task<Location?> GeocodeAddressAsync(
        string address,
        CancellationToken cancellationToken = default
    );
}
