using Microsoft.AspNetCore.SignalR;

public class OrderConfirmationService : IOrderConfirmationService
{
    private readonly IHubContext<FoodPlaceHub> _hubContext;
    private readonly IOrdersConfirmations _ordersConfirmations;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly TimeSpan _timeout;
    private readonly ILogger<OrderConfirmationService> _logger;

    public OrderConfirmationService(
        IHubContext<FoodPlaceHub> hubContext,
        IOrdersConfirmations ordersConfirmations,
        IFoodPlaceRepository foodPlaceRepository,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        ILogger<OrderConfirmationService> logger,
        TimeSpan? timeout = null
    )
    {
        _hubContext = hubContext;
        _ordersConfirmations = ordersConfirmations;
        _foodPlaceRepository = foodPlaceRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _logger = logger;
        _timeout = timeout ?? TimeSpan.FromSeconds(60);
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
                "Order ID: {OrderId} confirmation request finished without a response from food place {FoodPlaceId}.",
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

    public async Task<bool> ConfirmOrder(int orderId, int userId)
    {
        var order = await _orderRepository.GetOrderById(orderId);
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", orderId);
            return false;
        }

        var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
        if (foodPlaceUserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to accept order {OrderId} that belongs to food place user {FoodPlaceUserId}",
                userId,
                orderId,
                foodPlaceUserId
            );
            return false;
        }

        var orderConfirmation = _ordersConfirmations.GetOrderConfirmation(orderId);
        if (orderConfirmation == null)
        {
            _logger.LogWarning("Order confirmation not found for order ID {OrderId}", orderId);
            return false;
        }

        orderConfirmation.IsConfirmed = true;
        orderConfirmation.CancellationTokenSource.Cancel();

        _logger.LogInformation(
            "Order {OrderId} confirmed by food place user {UserId}",
            orderId,
            userId
        );
        return true;
    }

    public async Task<bool> RejectOrder(int orderId, int userId)
    {
        var order = await _orderRepository.GetOrderById(orderId);
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", orderId);
            return false;
        }

        var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
        if (foodPlaceUserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to reject order {OrderId} that belongs to food place user {FoodPlaceUserId}",
                userId,
                orderId,
                foodPlaceUserId
            );
            return false;
        }

        var orderConfirmation = _ordersConfirmations.GetOrderConfirmation(orderId);
        if (orderConfirmation == null)
        {
            _logger.LogWarning("Order confirmation not found for order ID {OrderId}", orderId);
            return false;
        }

        orderConfirmation.CancellationTokenSource.Cancel();

        _logger.LogInformation(
            "Order {OrderId} rejected by food place user {UserId}",
            orderId,
            userId
        );
        return true;
    }

    private async Task<OrderConfirmationDTO> CreateOrderConfirmationDTO(Order order)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(order.FoodPlaceId);
        var customer = await _userRepository.GetUserById(order.CustomerId);

        return new OrderConfirmationDTO
        {
            OrderId = order.Id,
            CustomerName = $"{customer!.FirstName} {customer.Surname}",
            OrderItems = order.Items.Select(item => new OrderConfirmationItemDTO
            {
                ItemName = foodPlace!.Items!.FirstOrDefault(i => i.Id == item.ItemId)!.Name,
                Quantity = item.Quantity,
            }),
        };
    }
}
