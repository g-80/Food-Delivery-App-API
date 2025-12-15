public class MapboxDirectionsResponse
{
    public required MapboxRouteInfo[] Routes { get; init; }
}

public class MapboxRouteInfo
{
    public required double Distance { get; init; }
    public required double Duration { get; init; }
    public required MapboxLegInfo[] Legs { get; init; }
}

public class MapboxLegInfo
{
    public required double Distance { get; init; }
    public required double Duration { get; init; }
}

public class MapboxGeocodingResponse
{
    public required MapboxFeature[] Features { get; init; }
}

public class MapboxFeature
{
    public required double[] Center { get; init; }
}
