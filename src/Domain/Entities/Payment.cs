public class Payment
{
    public string? StripePaymentIntentId { get; set; }
    public required PaymentStatuses Status { get; set; }
    public required int Amount { get; init; }
}
