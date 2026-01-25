using System.Text.Json;

public class RouteSimulationService : IRouteSimulationService
{
    private const double METERS_PER_MILE = 1609.344;
    private const double SECONDS_PER_HOUR = 3600.0;

    public IEnumerable<Location> ExtractCoordinates(string routeJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(routeJson);
            var routes = doc.RootElement.GetProperty("routes");

            if (routes.GetArrayLength() == 0)
                return Enumerable.Empty<Location>();

            var geometry = routes[0].GetProperty("geometry");
            var coordinates = geometry.GetProperty("coordinates");

            var locations = new List<Location>();
            foreach (var coord in coordinates.EnumerateArray())
            {
                var lon = coord[0].GetDouble();
                var lat = coord[1].GetDouble();

                locations.Add(new Location { Longitude = lon, Latitude = lat });
            }

            return locations;
        }
        catch (Exception)
        {
            return Enumerable.Empty<Location>();
        }
    }

    public IEnumerable<Location> CalculatePositionsAtSpeed(
        IEnumerable<Location> coordinates,
        double speedMph,
        TimeSpan updateInterval
    )
    {
        var coordList = coordinates.ToList();
        if (coordList.Count < 2)
            return coordList;

        var speedMetersPerSecond = speedMph * METERS_PER_MILE / SECONDS_PER_HOUR;
        var distancePerUpdate = speedMetersPerSecond * updateInterval.TotalSeconds;

        var positions = new List<Location>();

        var cumulativeDistances = new List<double> { 0 };
        for (int i = 1; i < coordList.Count; i++)
        {
            var distance = HaversineDistance(coordList[i - 1], coordList[i]);
            cumulativeDistances.Add(cumulativeDistances[i - 1] + distance);
        }

        var totalDistance = cumulativeDistances.Last();
        var currentDistance = 0.0;

        while (currentDistance < totalDistance)
        {
            var position = InterpolatePosition(coordList, cumulativeDistances, currentDistance);
            positions.Add(position);
            currentDistance += distancePerUpdate;
        }

        positions.Add(coordList.Last());

        return positions;
    }

    private Location InterpolatePosition(
        List<Location> coordinates,
        List<double> cumulativeDistances,
        double targetDistance
    )
    {
        int segmentIndex = 0;
        for (int i = 1; i < cumulativeDistances.Count; i++)
        {
            if (cumulativeDistances[i] >= targetDistance)
            {
                segmentIndex = i - 1;
                break;
            }
            segmentIndex = i - 1;
        }

        if (segmentIndex >= coordinates.Count - 1)
            return coordinates.Last();

        var segmentStart = coordinates[segmentIndex];
        var segmentEnd = coordinates[segmentIndex + 1];
        var segmentStartDistance = cumulativeDistances[segmentIndex];
        var segmentEndDistance = cumulativeDistances[segmentIndex + 1];

        var segmentLength = segmentEndDistance - segmentStartDistance;
        if (segmentLength < 0.001)
            return segmentStart;

        var ratio = (targetDistance - segmentStartDistance) / segmentLength;

        return new Location
        {
            Latitude =
                segmentStart.Latitude + (segmentEnd.Latitude - segmentStart.Latitude) * ratio,
            Longitude =
                segmentStart.Longitude + (segmentEnd.Longitude - segmentStart.Longitude) * ratio,
        };
    }

    private double HaversineDistance(Location loc1, Location loc2)
    {
        const double R = 6371000;

        var lat1Rad = loc1.Latitude * Math.PI / 180;
        var lat2Rad = loc2.Latitude * Math.PI / 180;
        var deltaLat = (loc2.Latitude - loc1.Latitude) * Math.PI / 180;
        var deltaLon = (loc2.Longitude - loc1.Longitude) * Math.PI / 180;

        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
            + Math.Cos(lat1Rad)
                * Math.Cos(lat2Rad)
                * Math.Sin(deltaLon / 2)
                * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}
