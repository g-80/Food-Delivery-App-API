public class UpdateETAHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<UpdateETAHandler> _logger;

    public UpdateETAHandler(IOrderRepository orderRepository, ILogger<UpdateETAHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateETACommand request)
    {
        // change name to order id or use the real deliveryId . theres mixing between the naming here

        var order = await _orderRepository.GetOrderById(request.DeliveryId);

        if (order?.Delivery == null)
        {
            _logger.LogWarning("Order with delivery ID {DeliveryId} not found", request.DeliveryId);
            throw new InvalidOperationException(
                $"Order with delivery ID {request.DeliveryId} not found"
            );
        }

        if (order.Delivery.DriverId != request.DriverId)
        {
            _logger.LogWarning(
                "Driver {DriverId} is not assigned to delivery {DeliveryId}",
                request.DriverId,
                request.DeliveryId
            );
            return;
        }

        var estimatedDeliveryTime = DateTime.UtcNow.Add(request.NewETA);

        _logger.LogInformation(
            "Updating ETA for delivery {DeliveryId} by driver {DriverId} to {EstimatedDeliveryTime}",
            request.DeliveryId,
            request.DriverId,
            estimatedDeliveryTime
        );
        // do something with the new ETA
    }
}
