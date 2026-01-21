public class OrderCancellationService : IOrderCancellationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;
    private readonly ILogger<OrderCancellationService> _logger;

    public OrderCancellationService(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IDeliveryAssignmentService deliveryAssignmentService,
        ILogger<OrderCancellationService> logger
    )
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _deliveryAssignmentService = deliveryAssignmentService;
        _logger = logger;
    }

    public async Task<bool> CancelOrder(Order order, string? reason = null)
    {
        _logger.LogInformation(
            "Cancelling order ID: {OrderId}. Reason: {Reason}",
            order.Id,
            reason ?? "Not specified"
        );

        await HandlePaymentCancellation(order);

        order.Status = OrderStatuses.cancelled;
        await _orderRepository.UpdateOrderStatus(order);

        _logger.LogInformation("Successfully cancelled order ID: {OrderId}", order.Id);
        return true;
    }

    private async Task HandlePaymentCancellation(Order order)
    {
        switch (order.Payment!.Status)
        {
            case PaymentStatuses.NotConfirmed
            or PaymentStatuses.PendingCapture:
                _logger.LogInformation(
                    "Cancelling payment intent for order ID: {OrderId}, payment intent: {PaymentIntentId}",
                    order.Id,
                    order.Payment.StripePaymentIntentId
                );
                _paymentService.CancelPaymentIntent(order.Payment.StripePaymentIntentId!);
                order.Payment.Status = PaymentStatuses.Cancelled;
                break;

            case PaymentStatuses.Completed:
                _logger.LogInformation(
                    "Refunding payment for order ID: {OrderId}, payment intent: {PaymentIntentId}, amount: {Amount} pence",
                    order.Id,
                    order.Payment.StripePaymentIntentId,
                    order.Total
                );
                _paymentService.RefundPayment(order.Payment.StripePaymentIntentId!, order.Total);
                order.Payment.Status = PaymentStatuses.Refunded;
                break;

            default:
                _logger.LogInformation(
                    "No payment action needed for order ID: {OrderId}, payment status: {PaymentStatus}",
                    order.Id,
                    order.Payment.Status
                );
                break;
        }

        if (
            order.Payment.Status == PaymentStatuses.Cancelled
            || order.Payment.Status == PaymentStatuses.Refunded
        )
        {
            await _orderRepository.UpdatePaymentStatus(order.Id, order.Payment);
        }
    }
}
