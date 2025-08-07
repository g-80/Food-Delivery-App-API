public class UpdateOrderStatusHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public UpdateOrderStatusHandler(
        IOrderRepository orderRepository,
        IFoodPlaceRepository foodPlaceRepository
    )
    {
        _orderRepository = orderRepository;
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand command, int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderById(orderId);
        if (order == null)
        {
            throw new Exception("Order not found");
        }

        var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
        if (foodPlaceUserId != userId)
        {
            return false;
        }

        var requiredStatus = (int)command.Status - 1;
        var allowedStatuses = new List<OrderStatuses>
        {
            OrderStatuses.preparing,
            OrderStatuses.readyForPickup,
            OrderStatuses.delivering,
            OrderStatuses.completed,
        };
        if (!allowedStatuses.Contains(command.Status) || (int)order.Status != requiredStatus)
        {
            throw new Exception("Invalid status update");
        }

        order.Status = command.Status;
        return await _orderRepository.UpdateOrderStatus(order);
    }
}
