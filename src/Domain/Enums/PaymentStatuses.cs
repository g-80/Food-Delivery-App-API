public enum PaymentStatuses
{
    NotConfirmed,
    PendingCapture,
    Completed,
    Failed = -1,
    Cancelled = -2,
    Refunded = -3,
}
