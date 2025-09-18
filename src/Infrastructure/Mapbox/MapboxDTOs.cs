public class MapboxDirectionsResponse
{
    public required MapboxRoute[] Routes { get; init; }
    public required MapboxWaypoint[] Waypoints { get; init; }
    public required string Code { get; init; }
}

public class MapboxRoute
{
    public required double Distance { get; init; }
    public required double Duration { get; init; }
    public required MapboxGeometry Geometry { get; init; }
    public required MapboxLeg[] Legs { get; init; }
}

public class MapboxGeometry
{
    public required string Type { get; init; }
    public required double[][] Coordinates { get; init; }
}

public class MapboxLeg
{
    public required double Distance { get; init; }
    public required double Duration { get; init; }
    public required MapboxStep[] Steps { get; init; }
}

public class MapboxStep
{
    public required string Name { get; init; }
    public required double Distance { get; init; }
    public required double Duration { get; init; }
    public required MapboxManeuver Maneuver { get; init; }
    public required MapboxGeometry Geometry { get; init; }
    public required MapboxBannerInstruction[] BannerInstructions { get; init; }
    public required MapboxVoiceInstruction[] VoiceInstructions { get; init; }
}

public class MapboxManeuver
{
    public required double[] Location { get; init; }
    public required string Type { get; init; }
    public string? Modifier { get; init; }
    public required string Instruction { get; init; }
    public required int BearingBefore { get; init; }
    public required int BearingAfter { get; init; }
}

public class MapboxWaypoint
{
    public required double[] Location { get; init; }
    public required string Name { get; init; }
}

public class MapboxBannerInstruction
{
    public required double DistanceAlongGeometry { get; init; }
    public required MapboxBannerContent Primary { get; init; }
    public MapboxBannerContent? Secondary { get; init; }
    public MapboxBannerContent? Sub { get; init; }
}

public class MapboxBannerContent
{
    public required string Text { get; init; }
    public MapboxBannerComponent[]? Components { get; init; }
    public required string Type { get; init; }
    public string? Modifier { get; init; }
    public int[]? Degrees { get; init; }
}

public class MapboxBannerComponent
{
    public required string Text { get; init; }
    public required string Type { get; init; }
    public string? Abbreviation { get; init; }
    public int? AbbreviationPriority { get; init; }
    public string? ImageBaseUrl { get; init; }
    public MapboxBannerDirections? Directions { get; init; }
}

public class MapboxBannerDirections
{
    public required string Text { get; init; }
    public MapboxBannerComponent[]? Components { get; init; }
}

public class MapboxVoiceInstruction
{
    public required double DistanceAlongGeometry { get; init; }
    public required string Announcement { get; init; }
    public string? SsmlAnnouncement { get; init; }
}

public class MapboxGeocodingResponse
{
    public required MapboxFeature[] Features { get; init; }
}

public class MapboxFeature
{
    public required double[] Center { get; init; }
    public required string PlaceName { get; init; }
}
