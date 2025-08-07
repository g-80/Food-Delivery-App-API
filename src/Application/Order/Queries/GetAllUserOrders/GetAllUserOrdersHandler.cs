public class GetAllUserOrdersHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;

    public GetAllUserOrdersHandler(
        IOrderRepository orderRepository,
        IFoodPlaceRepository foodPlaceRepository
    )
    {
        _orderRepository = orderRepository;
        _foodPlaceRepository = foodPlaceRepository;
    }

    public async Task<GetAllUserOrdersDTO> Handle(int userId)
    {
        var orders = await _orderRepository.GetAllOrdersByCustomerId(userId);
        if (orders == null || !orders.Any())
        {
            return new GetAllUserOrdersDTO { Orders = Enumerable.Empty<UserOrdersSummaryDTO>() };
        }

        var ordersDtos = orders.Select(async order =>
        {
            var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(order.FoodPlaceId);
            return MapToDTO(order, foodPlace!);
        });

        return new GetAllUserOrdersDTO { Orders = await Task.WhenAll(ordersDtos) };
    }

    private UserOrdersSummaryDTO MapToDTO(Order order, FoodPlace foodPlace)
    {
        return new UserOrdersSummaryDTO
        {
            OrderId = order.Id,
            FoodPlaceName = foodPlace.Name,
            Status = order.Status,
            Total = order.Total,
            ItemsCount = order.Items!.Count,
            CreatedAt = order.CreatedAt,
        };
    }
}
