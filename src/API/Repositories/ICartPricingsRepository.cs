using Npgsql;

public interface ICartPricingsRepository
{
    Task CreateCartPricing(CartPricingDTO dto, NpgsqlTransaction? transaction = null);
    Task<int> DeleteCartPricing(int id);
    Task<CartPricing?> GetCartPricingByCartId(int cartId);
    Task UpdateCartPricing(CartPricingDTO dto);
}
