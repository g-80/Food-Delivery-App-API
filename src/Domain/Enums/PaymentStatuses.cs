public enum PaymentStatuses
{
    NotConfirmed,
    PendingCapture,
    Completed,
    Cancelled = -1,
    Refunded = -2,
}
