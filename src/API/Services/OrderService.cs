public class OrderService : IOrderService
{
    private readonly IOrdersRepository _ordersRepo;
    private readonly IOrdersItemsRepository _ordersItemsRepo;
    private readonly ICartService _cartService;
    private readonly UnitOfWork _unitOfWork;

    public OrderService(
        IOrdersRepository ordersRepo,
        IOrdersItemsRepository ordersItemsRepository,
        ICartService cartService,
        UnitOfWork unitOfWork
    )
    {
        _ordersRepo = ordersRepo;
        _ordersItemsRepo = ordersItemsRepository;
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateOrderAsync(int customerId)
    {
        Cart? cart = await _cartService.GetCartByCustomerIdAsync(customerId);
        if (cart == null)
            throw new CartNotFoundException();

        var cartDetails = await _cartService.GetCartDetailsAsync(customerId);
        if (!cartDetails.CartItems.Any())
            throw new InvalidOperationException("Cart is empty");

        using (_unitOfWork)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                int orderId = await _ordersRepo.CreateOrder(
                    new CreateOrderDTO { CustomerId = customerId, TotalPrice = cartDetails.Total },
                    _unitOfWork.Transaction
                );

                foreach (var item in cartDetails.CartItems)
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
                        },
                        _unitOfWork.Transaction
                    );
                }
                await _cartService.ResetCartAsync(cart.Id);
                _unitOfWork.Commit();
                return orderId;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        return await _ordersRepo.CancelOrder(orderId);
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _ordersRepo.GetOrderById(orderId);
    }
}
