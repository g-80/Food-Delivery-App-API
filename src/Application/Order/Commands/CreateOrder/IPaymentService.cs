using Stripe;

public interface IPaymentService
{
    void CancelPaymentIntent(string paymentIntentId);
    void CapturePaymentIntent(string paymentIntentId);
    PaymentIntent CreatePaymentIntent(Order order, Address address);
    Task<Refund> GetRefundByPaymentIntentId(string paymentIntentId);
    void RefundPayment(string paymentIntentId, int amount);
}
