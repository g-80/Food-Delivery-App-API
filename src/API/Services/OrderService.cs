using System.Transactions;

public class OrderService : IOrderService
{
    private readonly IOrdersRepository _ordersRepo;
    private readonly IOrdersItemsRepository _ordersItemsRepo;
    private readonly ICartService _cartService;

    public OrderService(
        IOrdersRepository ordersRepo,
        IOrdersItemsRepository ordersItemsRepository,
        ICartService cartService
    )
    {
        _ordersRepo = ordersRepo;
        _ordersItemsRepo = ordersItemsRepository;
        _cartService = cartService;
    }

    public async Task<int> CreateOrderAsync(int customerId)
    {
        Cart cart =
            await _cartService.GetCartByCustomerIdAsync(customerId)
            ?? throw new CartNotFoundException();

        var cartItems = await _cartService.GetCartItemsByCartId(cart.Id);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty");

        var cartPricing =
            await _cartService.GetCartPricingByCartId(cart.Id)
            ?? throw new Exception($"Cart pricing for cart id = {cart.Id} was not found");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        int orderId = await _ordersRepo.CreateOrder(
            new CreateOrderDTO { CustomerId = customerId, TotalPrice = cartPricing.Total }
        );

        foreach (var item in cartItems)
        {
            await _ordersItemsRepo.CreateOrderItem(
                new CreateOrderItemDTO
                {
                    RequestedItem = new RequestedItem
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                    },
                    OrderId = orderId,
                    Subtotal = item.Subtotal,
                }
            );
        }
        await _cartService.ResetCartAsync(cart.Id);
        scope.Complete();
        return orderId;
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        return await _ordersRepo.CancelOrder(orderId);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId)
    {
        var order =
            await _ordersRepo.GetOrderById(orderId) ?? throw new Exception("Order was not found");
        return new OrderResponse { OrderId = order.Id, TotalPrice = order.TotalPrice };
    }
}
