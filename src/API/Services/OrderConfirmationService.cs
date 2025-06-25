using Microsoft.AspNetCore.SignalR;

public class OrderConfirmationService
{
    private readonly IHubContext<FoodPlaceHub> _hubContext;
    private readonly OrdersConfirmations _ordersConfirmations;
    private readonly IOrderService _orderService;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

    public OrderConfirmationService(
        IHubContext<FoodPlaceHub> hubContext,
        OrdersConfirmations ordersConfirmations,
        IOrderService orderService
    )
    {
        _hubContext = hubContext;
        _ordersConfirmations = ordersConfirmations;
        _orderService = orderService;
    }

    public async Task<bool> NotifyFoodPlaceAsync(int orderId)
    {
        var foodPlaceUserId = await _orderService.GetFoodPlaceUserIdAsync(orderId);
        var confirmationDto = await _orderService.GetOrderConfirmationDTOAsync(orderId);

        await _hubContext
            .Clients.User(foodPlaceUserId.ToString())
            .SendAsync("ReceiveOrderConfirmation", confirmationDto);

        var cts = new CancellationTokenSource();
        var orderConfirmation = new OrderConfirmation
        {
            CancellationTokenSource = cts,
            IsConfirmed = false,
        };
        _ordersConfirmations.AddOrderConfirmation(orderId, orderConfirmation);
        try
        {
            await Task.Delay(_timeout, cts.Token);
        }
        catch (TaskCanceledException) { }
        finally
        {
            _ordersConfirmations.RemoveOrderConfirmation(orderId);
            cts.Dispose();
        }
        return orderConfirmation.IsConfirmed;
    }

    public void ConfirmOrderAsync(int orderId)
    {
        var orderConfirmation = _ordersConfirmations.GetOrderConfirmation(orderId);
        orderConfirmation.IsConfirmed = true;
        orderConfirmation.CancellationTokenSource.Cancel();
    }

    public void RejectOrderAsync(int orderId)
    {
        var orderConfirmation = _ordersConfirmations.GetOrderConfirmation(orderId);
        orderConfirmation.CancellationTokenSource.Cancel();
    }
}
