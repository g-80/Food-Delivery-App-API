public class OrderService
{
    private readonly OrdersRepository _ordersRepo;
    private readonly OrdersItemsRepository _ordersItemsRepo;
    private readonly CartService _cartService;
    private readonly CartsRepository _cartsRepo;
    private readonly UnitOfWork _unitOfWork;

    public OrderService(
        OrdersRepository ordersRepo,
        OrdersItemsRepository ordersItemsRepository,
        CartsRepository cartsRepository,
        CartService cartService,
        UnitOfWork unitOfWork)
    {
        _ordersRepo = ordersRepo;
        _ordersItemsRepo = ordersItemsRepository;
        _cartsRepo = cartsRepository;
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateOrderAsync(int customerId)
    {
        Cart? cart = await _cartService.GetCartByCustomerIdAsync(customerId);
        if (cart == null)
            throw new CartNotFoundException();
        if (cart!.IsUsed)
            throw new Exception("Cart has already been checked out");

        var cartDetails = await _cartService.GetCartDetailsAsync(customerId);
        if (!cartDetails.CartItems.Any())
            throw new EmptyCartException();

        using (_unitOfWork)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                await _cartsRepo.SetCartAsUsed(cart.Id, _unitOfWork.Transaction);

                int orderId = await _ordersRepo.CreateOrder(new CreateOrderDTO
                {
                    CustomerId = customerId,
                    TotalPrice = cartDetails.Total
                },
                _unitOfWork.Transaction);

                foreach (var item in cartDetails.CartItems)
                {
                    await _ordersItemsRepo.CreateOrderItem(
                        new CreateOrderItemDTO
                        {
                            RequestedItem = new RequestedItem { ItemId = item.ItemId, Quantity = item.Quantity },
                            OrderId = orderId,
                            Subtotal = item.Subtotal
                        },
                        _unitOfWork.Transaction
                    );
                }
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
