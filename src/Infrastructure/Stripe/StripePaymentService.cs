using Stripe;

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(ILogger<StripePaymentService> logger, IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration.GetValue<string>("Stripe:SecretKey");
        _logger = logger;
    }

    public PaymentIntent CreatePaymentIntent(Order order, Address address)
    {
        var options = new PaymentIntentCreateOptions()
        {
            Amount = order.Total,
            Currency = "gbp",
            CaptureMethod = "manual",
            Shipping = new ChargeShippingOptions
            {
                Name = "Customer",
                Address = new AddressOptions
                {
                    Line1 = address.NumberAndStreet,
                    City = address.City,
                    PostalCode = address.Postcode,
                },
            },
            Metadata = new Dictionary<string, string>
            {
                { "customer_id", order.CustomerId.ToString() },
                { "order_id", order.Id.ToString() },
            },
        };

        var service = new PaymentIntentService();
        PaymentIntent intent = service.Create(options);
        _logger.LogInformation("Created payment intent with id: {PaymentIntentId}", intent.Id);
        return intent;
    }

    public void CancelPaymentIntent(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        service.Cancel(paymentIntentId);
        _logger.LogInformation(
            "Cancelled payment intent with id: {PaymentIntentId}",
            paymentIntentId
        );
    }

    public void CapturePaymentIntent(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        service.Capture(paymentIntentId);
        _logger.LogInformation(
            "Captured payment intent with id: {PaymentIntentId}",
            paymentIntentId
        );
    }
}
