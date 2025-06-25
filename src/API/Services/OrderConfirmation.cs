public class OrderConfirmation
{
    public required CancellationTokenSource CancellationTokenSource { get; init; }
    public required bool IsConfirmed { get; set; }
}
