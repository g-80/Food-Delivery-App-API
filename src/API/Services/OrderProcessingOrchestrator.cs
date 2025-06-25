using Hangfire;
using Microsoft.AspNetCore.SignalR;

public class OrderProcessingOrchestrator
{
    private readonly IOrderService _orderService;
    private readonly DeliveriesService _deliveriesService;
    private readonly DeliveryAssignmentService _deliveryAssignmentService;
    private readonly OrderConfirmationService _orderConfirmationService;

    public OrderProcessingOrchestrator(
        IOrderService orderService,
        DeliveriesService deliveriesService,
        DeliveryAssignmentService deliveryAssignmentService,
        OrderConfirmationService orderConfirmationService
    )
    {
        _orderService = orderService;
        _deliveriesService = deliveriesService;
        _deliveryAssignmentService = deliveryAssignmentService;
        _orderConfirmationService = orderConfirmationService;
    }

    public async Task<List<int>> CreateOrderAsync(int customerId, OrderCreateRequest request)
    {
        List<int> orderIds = await _orderService.CreateOrderAsync(customerId, request);
        foreach (var orderId in orderIds)
        {
            BackgroundJob.Enqueue(() => ProcessOrderAsync(orderId));
        }
        return orderIds;
    }

    public async Task ProcessOrderAsync(int orderId)
    {
        var isConfirmed = await _orderConfirmationService.NotifyFoodPlaceAsync(orderId);

        if (!isConfirmed)
        {
            await _orderService.CancelOrderAsync(orderId);
            return;
        }
        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatuses.preparing);
        await _deliveriesService.CreateDeliveryAsync(orderId);

        await _deliveryAssignmentService.AssignDeliveryToADriver(orderId);
    }
}
