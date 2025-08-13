public class CancelOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IFoodPlaceRepository foodPlaceRepository
    )
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _foodPlaceRepository = foodPlaceRepository;
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
            || (order.Status != OrderStatuses.pending && order.Status != OrderStatuses.preparing)
        )
        {
            return false;
        }

        order.Status = OrderStatuses.cancelled;
        // store the reason for cancellation
        // stop any ongoing delivery process
        return await _orderRepository.UpdateOrderStatus(order);
    }
}
