public interface IDriverPaymentService
{
    int CalculatePayment(double distanceInMeters, double durationInSeconds);
}