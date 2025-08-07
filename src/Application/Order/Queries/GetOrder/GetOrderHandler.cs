public class GetOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IAddressRepository _addressRepository;

    public GetOrderHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository
    )
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _foodPlaceRepository = foodPlaceRepository;
        _addressRepository = addressRepository;
    }

    public async Task<GetOrderDTO?> Handle(int orderId, int userId)
    {
        var order = await _orderRepository.GetOrderById(orderId);
        if (order == null)
        {
            throw new Exception("Order not found");
        }

        var user = await _userRepository.GetUserById(userId);
        if (user!.UserType == UserTypes.food_place)
        {
            var foodPlaceUserId = await _foodPlaceRepository.GetFoodPlaceUserId(order.FoodPlaceId);
            if (foodPlaceUserId != userId)
            {
                return null;
            }
        }
        else if (order.CustomerId != userId)
        {
            return null;
        }

        var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(order.FoodPlaceId);
        return await MapToGetOrderDTO(order, foodPlace!);
    }

    private async Task<GetOrderDTO> MapToGetOrderDTO(Order order, FoodPlace foodPlace)
    {
        return new GetOrderDTO
        {
            OrderId = order.Id,
            FoodPlaceName = foodPlace.Name,
            FoodPlaceId = order.FoodPlaceId,
            Status = order.Status,
            DeliveryAddress = (await _addressRepository.GetAddressById(order.DeliveryAddressId))!,
            Subtotal = order.Subtotal,
            Fees = order.ServiceFee,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            Items = order
                .Items!.Select(item => new OrderItemDTO
                {
                    ItemId = item.ItemId,
                    ItemName = foodPlace.Items!.FirstOrDefault(i => i.Id == item.ItemId)!.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal,
                })
                .ToList(),
            CreatedAt = order.CreatedAt,
        };
    }
}
