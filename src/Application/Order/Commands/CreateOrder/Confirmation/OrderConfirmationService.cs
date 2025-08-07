using Microsoft.AspNetCore.SignalR;

public class OrderConfirmationService : IOrderConfirmationService
{
    private readonly IHubContext<FoodPlaceHub> _hubContext;
    private readonly OrdersConfirmations _ordersConfirmations;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

    public OrderConfirmationService(
        IHubContext<FoodPlaceHub> hubContext,
        OrdersConfirmations ordersConfirmations,
        IFoodPlaceRepository foodPlaceRepository,
        IUserRepository userRepository
    )
    {
        _hubContext = hubContext;
        _ordersConfirmations = ordersConfirmations;
        _foodPlaceRepository = foodPlaceRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> RequestOrderConfirmation(Order order)
    {
        var confirmationDto = await CreateOrderConfirmationDTO(order);

        var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
        await _hubContext
            .Clients.User(foodPlaceUserId.ToString()!)
            .SendAsync("ReceiveOrderConfirmation", confirmationDto);

        var cts = new CancellationTokenSource();
        var orderConfirmation = new OrderConfirmation
        {
            CancellationTokenSource = cts,
            IsConfirmed = false,
        };
        _ordersConfirmations.AddOrderConfirmation(order.Id, orderConfirmation);
        try
        {
            await Task.Delay(_timeout, cts.Token);
        }
        catch (TaskCanceledException) { }
        finally
        {
            _ordersConfirmations.RemoveOrderConfirmation(order.Id);
            cts.Dispose();
        }
        return orderConfirmation.IsConfirmed;
    }

    public void ConfirmOrder(int orderId)
    {
        var orderConfirmation = _ordersConfirmations.GetOrderConfirmation(orderId);
        orderConfirmation.IsConfirmed = true;
        orderConfirmation.CancellationTokenSource.Cancel();
    }

    public void RejectOrder(int orderId)
    {
        var orderConfirmation = _ordersConfirmations.GetOrderConfirmation(orderId);
        orderConfirmation.CancellationTokenSource.Cancel();
    }

    private async Task<OrderConfirmationDTO> CreateOrderConfirmationDTO(Order order)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(order.FoodPlaceId);
        var customer = await _userRepository.GetUserById(order.CustomerId);

        return new OrderConfirmationDTO
        {
            OrderId = order.Id,
            CustomerName = $"{customer!.FirstName} {customer.Surname}",
            OrderItems = order.Items!.Select(item => new OrderConfirmationItemDTO
            {
                ItemName = foodPlace!.Items!.FirstOrDefault(i => i.Id == item.ItemId)!.Name,
                Quantity = item.Quantity,
            }),
        };
    }
}
