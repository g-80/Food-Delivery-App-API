public class ProcessOrderHandler
{
    private readonly IOrderRepository _ordersRepository;
    private readonly IDemoOrderProcessingService _demoOrderProcessingService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessOrderHandler> _logger;

    public ProcessOrderHandler(
        IOrderRepository ordersRepository,
        IDemoOrderProcessingService demoOrderProcessingService,
        IPaymentService paymentService,
        ILogger<ProcessOrderHandler> logger
    )
    {
        _ordersRepository = ordersRepository;
        _demoOrderProcessingService = demoOrderProcessingService;
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

        _logger.LogInformation("Processing order ID: {OrderId} in demo mode", order.Id);

        _paymentService.CapturePaymentIntent(order.Payment.StripePaymentIntentId!);
        order.Payment.Status = PaymentStatuses.Completed;
        await _ordersRepository.UpdatePaymentStatus(order.Id, order.Payment);
        _logger.LogInformation(
            "Payment for Order ID: {OrderId} was successfully captured",
            order.Id
        );

        await _demoOrderProcessingService.ProcessDemoOrder(order);
    }
}
