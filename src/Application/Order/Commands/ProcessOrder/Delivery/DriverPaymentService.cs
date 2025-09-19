public class DriverPaymentService : IDriverPaymentService
{
    private const int BasePay = 300;
    private const int DistancePayPerKm = 30;
    private const int TimePayPerMinute = 20;

    public int CalculatePayment(double distanceInMeters, double durationInSeconds)
    {
        var distanceInKm = distanceInMeters / 1000.0;
        var durationInMinutes = durationInSeconds / 60.0;

        var distancePay = Convert.ToInt32(distanceInKm * DistancePayPerKm);
        var timePay = Convert.ToInt32(durationInMinutes * TimePayPerMinute);

        return BasePay + distancePay + timePay;
    }
}
