/// <summary>
/// Class <c>JourneyCalculationService</c> returns random dummy data
/// to simulate delivery journey data.
/// </summary>
public class JourneyCalculationService
{
    public int CalculateDistanceToDestination(
        double srcLatitude = 0,
        double srcLongitude = 0,
        double destLatitude = 0,
        double destLongitude = 0
    )
    {
        Random rnd = new Random();
        return rnd.Next(500, 1501);
    }

    public TimeSpan CalculateEstimatedTimeToDestination(
        double srcLatitude = 0,
        double srcLongitude = 0,
        double destLatitude = 0,
        double destLongitude = 0
    )
    {
        Random rnd = new Random();
        return TimeSpan.FromMinutes(rnd.Next(10, 31));
    }
}
