using System.Transactions;

public class OrderService : IOrderService
{
    private readonly IOrdersRepository _ordersRepo;
    private readonly IOrdersItemsRepository _ordersItemsRepo;
    private readonly ICartService _cartService;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly AddressesService _addressService;

    public OrderService(
        IOrdersRepository ordersRepo,
        IOrdersItemsRepository ordersItemsRepository,
        ICartService cartService,
        ICartItemsRepository cartItemsRepository,
        AddressesService addressService
    )
    {
        _ordersRepo = ordersRepo;
        _ordersItemsRepo = ordersItemsRepository;
        _cartService = cartService;
        _cartItemsRepo = cartItemsRepository;
        _addressService = addressService;
    }

    public async Task<List<int>> CreateOrderAsync(int customerId, OrderCreateRequest request)
    {
        var addressId = await _addressService.CreateAddress(request.DeliveryAddress, customerId);

        Cart cart =
            await _cartService.GetCartByCustomerIdAsync(customerId)
            ?? throw new CartNotFoundException();

        var cartItems = await _cartItemsRepo.GetCartItemsDetailsByCartId(cart.Id);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty");
        var cartItemsByFoodPlace = cartItems.GroupBy(item => item.FoodPlaceId);

        var cartPricing =
            await _cartService.GetCartPricingByCartId(cart.Id)
            ?? throw new Exception($"Cart pricing for cart id = {cart.Id} was not found");

        List<int> ordersIds = new();
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            foreach (var group in cartItemsByFoodPlace)
            {
                int orderId = await _ordersRepo.CreateOrder(
                    new CreateOrderDTO
                    {
                        CustomerId = customerId,
                        TotalPrice = cartPricing.Total,
                        FoodPlaceId = group.Key,
                        DeliveryAddressId = addressId,
                        Status = OrderStatuses.pending,
                    }
                );
                ordersIds.Add(orderId);

                foreach (var item in group)
                {
                    await _ordersItemsRepo.CreateOrderItem(
                        new CreateOrderItemDTO
                        {
                            OrderId = orderId,
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Subtotal = item.Subtotal,
                        }
                    );
                }
            }
            await _cartService.ResetCartAsync(cart.Id);
            scope.Complete();
        }

        return ordersIds;
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        return await _ordersRepo.UpdateOrderStatus(orderId, OrderStatuses.cancelled);
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _ordersRepo.GetOrderById(orderId)
            ?? throw new Exception("Order was not found");
    }

    public async Task<OrderResponse?> GetOrderResponseByIdAsync(int orderId)
    {
        var order =
            await _ordersRepo.GetOrderById(orderId) ?? throw new Exception("Order was not found");
        return new OrderResponse { OrderId = order.Id, TotalPrice = order.TotalPrice };
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatuses status)
    {
        if (status == OrderStatuses.cancelled)
        {
            return await CancelOrderAsync(orderId);
        }

        return await _ordersRepo.UpdateOrderStatus(orderId, status);
    }

    public async Task<OrderConfirmationDTO> GetOrderConfirmationDTOAsync(int orderId)
    {
        return await _ordersRepo.GetOrderConfirmationDTO(orderId);
    }

    public async Task<int> GetFoodPlaceUserIdAsync(int orderId)
    {
        var userId = await _ordersRepo.GetFoodPlaceUserIdAsync(orderId);
        if (userId == 0)
        {
            throw new Exception("Food place user id was not found for the order");
        }
        return userId;
    }
}
