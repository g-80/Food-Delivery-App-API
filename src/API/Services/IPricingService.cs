public interface IPricingService
{
    Task<CartPricingDTO> CalculateCartPricing(int cartId);
    Task<(int, int)> CalculateItemPriceAsync(RequestedItem requestedItem);
}
