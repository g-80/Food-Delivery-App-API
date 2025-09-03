public enum OrderStatuses
{
    pendingPayment,
    pendingConfirmation,
    preparing,
    readyForPickup,
    delivering,
    completed,
    cancelled = -1,
}
