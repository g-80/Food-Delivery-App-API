public interface IRouteSimulationService
{
    IEnumerable<Location> ExtractCoordinates(string routeJson);
    IEnumerable<Location> CalculatePositionsAtSpeed(
        IEnumerable<Location> coordinates,
        double speedMph,
        TimeSpan updateInterval
    );
}
