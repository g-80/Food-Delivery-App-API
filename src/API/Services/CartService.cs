public class CartService
{
    private readonly CartsRepository _cartsRepo;
    private readonly CartItemsRepository _cartItemsRepo;
    private readonly CartPricingsRepository _cartPricingsRepo;
    private readonly PricingService _pricingService;
    private readonly UnitOfWork _unitOfWork;

    public CartService(
        CartsRepository cartsRepository,
        CartItemsRepository cartItemsRepository,
        CartPricingsRepository cartPricingsRepository,
        PricingService pricingService,
        UnitOfWork unitOfWork)
    {
        _cartsRepo = cartsRepository;
        _cartItemsRepo = cartItemsRepository;
        _cartPricingsRepo = cartPricingsRepository;
        _pricingService = pricingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateCartAsync(int customerId)
    {
        using (_unitOfWork)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                int cartId = await _cartsRepo.CreateCart(
                    new CreateCartDTO
                    {
                        CustomerId = customerId,
                        Expiry = DateTime.UtcNow.AddMinutes(5)
                    },
                    _unitOfWork.Transaction);
                await _cartPricingsRepo.CreateCartPricing(
                    new CartPricingDTO
                    {
                        CartId = cartId,
                        Subtotal = 0,
                        Fees = 0,
                        DeliveryFee = 0,
                        Total = 0,
                    },
                    _unitOfWork.Transaction);

                _unitOfWork.Commit();
                return cartId;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }

    public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
    {
        return await _cartsRepo.GetCartByCustomerId(customerId) ?? throw new CartNotFoundException();
    }

    public async Task AddItemToCartAsync(AddItemToCartRequest req)
    {
        var cart = await GetCartByCustomerIdAsync(req.CustomerId);
        var (unitPrice, subtotal) = await _pricingService.CalculateItemPriceAsync(req.Item);
        await _cartItemsRepo.CreateCartItem(
            new CreateCartItemDTO
            {
                RequestedItem = req.Item,
                CartId = cart.Id,
                UnitPrice = unitPrice,
                Subtotal = subtotal
            }
        );
        await UpdateCartPricingAsync(cart.Id);
    }

    public async Task RemoveItemFromCartAsync(int customerId, int itemId)
    {
        var cart = await GetCartByCustomerIdAsync(customerId);
        await _cartItemsRepo.DeleteCartItem(cart.Id, itemId);
        await UpdateCartPricingAsync(cart.Id);
    }

    public async Task<CartResponse> GetCartDetailsAsync(int customerId)
    {
        var cart = await GetCartByCustomerIdAsync(customerId);
        IEnumerable<CartItem> cartItems = await _cartItemsRepo.GetCartItemsByCartId(cart.Id);
        IEnumerable<CartItemResponse> cartItemsResponse = cartItems.Select(item => new CartItemResponse { ItemId = item.ItemId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, Subtotal = item.Subtotal });
        var cartPricing = await _cartPricingsRepo.GetCartPricingByCartId(cart.Id) ?? throw new Exception("Cart pricing matching cart id was not found");
        return new CartResponse
        {
            CartItems = cartItemsResponse,
            Subtotal = cartPricing.Subtotal,
            Fees = cartPricing.Fees,
            DeliveryFee = cartPricing.DeliveryFee,
            Total = cartPricing.Total
        };
    }

    public async Task<bool> SetCartAsUsedAsync(int id)
    {
        return await _cartsRepo.SetCartAsUsed(id);
    }

    public async Task UpdateCartItemQuantityAsync(int customerId, int itemId, UpdateCartItemQuantityRequest req)
    {
        var cart = await GetCartByCustomerIdAsync(customerId);
        var (_, subtotal) = await _pricingService.CalculateItemPriceAsync(new RequestedItem { ItemId = itemId, Quantity = req.Quantity });
        await _cartItemsRepo.UpdateCartItemQuantity(cart.Id, itemId, req.Quantity, subtotal);
        await UpdateCartPricingAsync(cart.Id);
    }

    private async Task UpdateCartPricingAsync(int cartId)
    {
        var cartPricing = await _pricingService.CalculateCartPricing(cartId);
        await _cartPricingsRepo.UpdateCartPricing(cartPricing);
    }
}