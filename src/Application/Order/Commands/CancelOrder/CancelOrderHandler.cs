public class CancelOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IOrderCancellationService _orderCancellationService;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IOrderCancellationService orderCancellationService
    )
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _foodPlaceRepository = foodPlaceRepository;
        _orderCancellationService = orderCancellationService;
    }

    public async Task<bool> Handle(CancelOrderCommand command, int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderById(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        var user = await _userRepository.GetUserById(userId);
        if (user!.UserType == UserTypes.food_place)
        {
            var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
            if (foodPlaceUserId != userId)
            {
                return false;
            }
        }
        else if (
            order.CustomerId != userId
            || (
                order.Status != OrderStatuses.pendingConfirmation
                && order.Status != OrderStatuses.preparing
            )
            || order.Delivery?.Status != DeliveryStatuses.assigningDriver
        )
        {
            return false;
        }

        return await _orderCancellationService.CancelOrder(order, command.Reason);
    }
}
