using Microsoft.AspNetCore.SignalR;

public class OrderConfirmationService : IOrderConfirmationService
{
    private readonly IHubContext<FoodPlaceHub> _hubContext;
    private readonly OrdersConfirmations _ordersConfirmations;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);
    private readonly ILogger<OrderConfirmationService> _logger;

    public OrderConfirmationService(
        IHubContext<FoodPlaceHub> hubContext,
        OrdersConfirmations ordersConfirmations,
        IFoodPlaceRepository foodPlaceRepository,
        IUserRepository userRepository,
        ILogger<OrderConfirmationService> logger
    )
    {
        _hubContext = hubContext;
        _ordersConfirmations = ordersConfirmations;
        _foodPlaceRepository = foodPlaceRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> RequestOrderConfirmation(Order order)
    {
        var confirmationDto = await CreateOrderConfirmationDTO(order);

        var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
        _logger.LogInformation(
            "Requesting order confirmation for order ID: {OrderId} to food place user ID: {FoodPlaceUserId}",
            order.Id,
            foodPlaceUserId
        );
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
            _logger.LogInformation(
                "Waiting for confirmation for order ID: {OrderId} with timeout of {Timeout} seconds",
                order.Id,
                _timeout.TotalSeconds
            );
            await Task.Delay(_timeout, cts.Token);
            _logger.LogInformation(
                "Order ID: {OrderId} confirmation request completed without a response from food place {FoodPlaceId}.",
                order.Id,
                order.FoodPlaceId
            );
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation(
                "Order ID: {OrderId} confirmation request received a response from food place {FoodPlaceId}. Timeout stopped.",
                order.Id,
                order.FoodPlaceId
            );
        }
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
