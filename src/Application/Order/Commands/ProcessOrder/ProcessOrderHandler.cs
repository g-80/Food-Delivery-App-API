public class ProcessOrderHandler
{
    private readonly IOrderRepository _ordersRepository;
    private readonly IOrderConfirmationService _orderConfirmationService;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessOrderHandler> _logger;

    public ProcessOrderHandler(
        IOrderRepository ordersRepository,
        IOrderConfirmationService orderConfirmationService,
        IDeliveryAssignmentService deliveryAssignmentService,
        IPaymentService paymentService,
        ILogger<ProcessOrderHandler> logger
    )
    {
        _ordersRepository = ordersRepository;
        _orderConfirmationService = orderConfirmationService;
        _deliveryAssignmentService = deliveryAssignmentService;
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
            order.Status = OrderStatuses.cancelled;
            await _ordersRepository.UpdateOrderStatus(order);
            _logger.LogInformation(
                "Order ID: {OrderId} was cancelled after confirmation failed",
                order.Id
            );
            _paymentService.CancelPaymentIntent(order.Payment.StripePaymentIntentId!);
            order.Payment.Status = PaymentStatuses.Cancelled;
            await _ordersRepository.UpdatePaymentStatus(order.Id, order.Payment);
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
            order.Status = OrderStatuses.cancelled;
            await _ordersRepository.UpdateOrderStatus(order);
            _logger.LogInformation(
                "Order ID: {OrderId} was cancelled after delivery assignment failed",
                order.Id
            );
            _paymentService.CancelPaymentIntent(order.Payment.StripePaymentIntentId!);
            order.Payment.Status = PaymentStatuses.Cancelled;
            await _ordersRepository.UpdatePaymentStatus(order.Id, order.Payment);
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
