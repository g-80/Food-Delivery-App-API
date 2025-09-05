public class ProcessOrderHandler
{
    private readonly IOrderRepository _ordersRepository;
    private readonly IOrderConfirmationService _orderConfirmationService;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;
    private readonly IOrderCancellationService _orderCancellationService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessOrderHandler> _logger;

    public ProcessOrderHandler(
        IOrderRepository ordersRepository,
        IOrderConfirmationService orderConfirmationService,
        IDeliveryAssignmentService deliveryAssignmentService,
        IOrderCancellationService orderCancellationService,
        IPaymentService paymentService,
        ILogger<ProcessOrderHandler> logger
    )
    {
        _ordersRepository = ordersRepository;
        _orderConfirmationService = orderConfirmationService;
        _deliveryAssignmentService = deliveryAssignmentService;
        _orderCancellationService = orderCancellationService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Handle(int orderId)
    {
        var order = await _ordersRepository.GetOrderById(orderId);

        order!.Status = OrderStatuses.pendingConfirmation;
        await _ordersRepository.UpdateOrderStatus(order);
        order.Payment!.Status = PaymentStatuses.PendingCapture;
        await _ordersRepository.UpdatePaymentStatus(order.Id, order.Payment);

        _logger.LogInformation("Processing order ID: {OrderId}", order.Id);

        var isConfirmed = await _orderConfirmationService.RequestOrderConfirmation(order);

        if (!isConfirmed)
        {
            await _orderCancellationService.CancelOrder(order, "Confirmation failed");
            _logger.LogInformation(
                "Order ID: {OrderId} was cancelled after confirmation failed",
                order.Id
            );
            return;
        }

        _logger.LogInformation(
            "Order ID: {OrderId} confirmed, proceeding to preparation",
            order.Id
        );
        order.Status = OrderStatuses.preparing;
        await _ordersRepository.UpdateOrderStatus(order);

        order.CreateDelivery();
        await _ordersRepository.AddDelivery(order.Id, order.Delivery!);

        var result = await _deliveryAssignmentService.InitiateDeliveryAssignment(order);
        if (!result)
        {
            await _orderCancellationService.CancelOrder(order, "Delivery assignment failed");
            _logger.LogInformation(
                "Order ID: {OrderId} was cancelled after delivery assignment failed",
                order.Id
            );
            return;
        }
        _paymentService.CapturePaymentIntent(order.Payment.StripePaymentIntentId!);
        order.Payment.Status = PaymentStatuses.Completed;
        await _ordersRepository.UpdatePaymentStatus(order.Id, order.Payment);
        _logger.LogInformation(
            "Payment for Order ID: {OrderId} was successfully captured",
            order.Id
        );
    }
}
