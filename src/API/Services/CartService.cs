using System.Transactions;

public class CartService : ICartService
{
    private readonly ICartsRepository _cartsRepo;
    private readonly ICartItemsRepository _cartItemsRepo;
    private readonly ICartPricingsRepository _cartPricingsRepo;
    private readonly IPricingService _pricingService;
    private readonly TimeSpan _cartExpirationTime = TimeSpan.FromMinutes(5);

    public CartService(
        ICartsRepository cartsRepository,
        ICartItemsRepository cartItemsRepository,
        ICartPricingsRepository cartPricingsRepository,
        IPricingService pricingService
    )
    {
        _cartsRepo = cartsRepository;
        _cartItemsRepo = cartItemsRepository;
        _cartPricingsRepo = cartPricingsRepository;
        _pricingService = pricingService;
    }

    public async Task<int> CreateCartAsync(int customerId)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            int cartId = await _cartsRepo.CreateCart(
                new CreateCartDTO
                {
                    CustomerId = customerId,
                    Expiry = DateTime.UtcNow.Add(_cartExpirationTime),
                }
            );

            await _cartPricingsRepo.CreateCartPricing(
                new CartPricingDTO
                {
                    CartId = cartId,
                    Subtotal = 0,
                    Fees = 0,
                    DeliveryFee = 0,
                    Total = 0,
                }
            );

            scope.Complete();
            return cartId;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
    {
        var cart =
            await _cartsRepo.GetCartByCustomerId(customerId) ?? throw new CartNotFoundException();
        if (cart.ExpiresAt < DateTime.UtcNow)
        {
            await RefreshCart(cart.Id);
        }

        return cart;
    }

    public async Task<CartResponse> GetCartDetailsAsync(int customerId)
    {
        var cart = await GetCartByCustomerIdAsync(customerId);
        IEnumerable<CartItem> cartItems = await _cartItemsRepo.GetCartItemsByCartId(cart.Id);
        IEnumerable<CartItemResponse> cartItemsResponse = cartItems.Select(
            item => new CartItemResponse
            {
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal,
            }
        );
        var cartPricing =
            await _cartPricingsRepo.GetCartPricingByCartId(cart.Id)
            ?? throw new Exception("Cart pricing matching cart id was not found");
        return new CartResponse
        {
            CartItems = cartItemsResponse,
            Subtotal = cartPricing.Subtotal,
            Fees = cartPricing.Fees,
            DeliveryFee = cartPricing.DeliveryFee,
            Total = cartPricing.Total,
        };
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsByCartId(int cartId)
    {
        return await _cartItemsRepo.GetCartItemsByCartId(cartId);
    }

    public async Task<CartPricing?> GetCartPricingByCartId(int cartId)
    {
        return await _cartPricingsRepo.GetCartPricingByCartId(cartId);
    }

    public async Task AddItemToCartAsync(int customerId, CartAddItemRequest req)
    {
        var cart = await GetCartByCustomerIdAsync(customerId);
        var existingItem = (await _cartItemsRepo.GetCartItemsByCartId(cart.Id)).FirstOrDefault(i =>
            i.ItemId == req.Item.ItemId
        );

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        if (existingItem != null)
        {
            int newQuantity = existingItem.Quantity + req.Item.Quantity;
            var combinedItem = new RequestedItem
            {
                ItemId = req.Item.ItemId,
                Quantity = newQuantity,
            };
            var (unitPrice, subtotal) = await _pricingService.CalculateItemPriceAsync(combinedItem);

            await _cartItemsRepo.UpdateCartItemQuantity(
                cart.Id,
                req.Item.ItemId,
                newQuantity,
                subtotal
            );
        }
        else
        {
            var (unitPrice, subtotal) = await _pricingService.CalculateItemPriceAsync(req.Item);
            await _cartItemsRepo.CreateCartItem(
                new CreateCartItemDTO
                {
                    RequestedItem = req.Item,
                    CartId = cart.Id,
                    UnitPrice = unitPrice,
                    Subtotal = subtotal,
                }
            );
        }

        await RefreshCart(cart.Id);
        scope.Complete();
    }

    public async Task RemoveItemFromCartAsync(int customerId, int itemId)
    {
        var cart = await GetCartByCustomerIdAsync(customerId);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _cartItemsRepo.DeleteCartItem(cart.Id, itemId);
        await RefreshCart(cart.Id);
        scope.Complete();
    }

    public async Task UpdateCartItemQuantityAsync(
        int customerId,
        int itemId,
        CartUpdateItemQuantityRequest req
    )
    {
        var cart = await GetCartByCustomerIdAsync(customerId);

        var (_, subtotal) = await _pricingService.CalculateItemPriceAsync(
            new RequestedItem { ItemId = itemId, Quantity = req.Quantity }
        );

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _cartItemsRepo.UpdateCartItemQuantity(cart.Id, itemId, req.Quantity, subtotal);

        await RefreshCart(cart.Id);
        scope.Complete();
    }

    public async Task ResetCartAsync(int cartId)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await RemoveAllItemsFromCartAsync(cartId);
        await RecalculateCartPricingAsync(cartId);

        scope.Complete();
    }

    private async Task RefreshCart(int cartId)
    {
        await RecalculateCartPricingAsync(cartId);
        await _cartsRepo.UpdateCartExpiry(cartId, DateTime.UtcNow.Add(_cartExpirationTime));
    }

    private async Task RecalculateCartPricingAsync(int cartId)
    {
        var cartItems = await _cartItemsRepo.GetCartItemsByCartId(cartId);

        foreach (var item in cartItems)
        {
            var requestedItem = new RequestedItem
            {
                ItemId = item.ItemId,
                Quantity = item.Quantity,
            };
            var (unitPrice, subtotal) = await _pricingService.CalculateItemPriceAsync(
                requestedItem
            );

            if (subtotal != item.Subtotal)
            {
                await _cartItemsRepo.UpdateCartItemPrice(cartId, item.ItemId, unitPrice, subtotal);
            }
        }

        var cartPricing = await _pricingService.CalculateCartPricing(cartId);
        await _cartPricingsRepo.UpdateCartPricing(cartPricing);
    }

    private async Task RemoveAllItemsFromCartAsync(int cartId)
    {
        await _cartItemsRepo.DeleteAllCartItemsByCartId(cartId);
    }
}
