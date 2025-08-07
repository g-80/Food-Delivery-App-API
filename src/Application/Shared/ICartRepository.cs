public interface ICartRepository
{
    Task AddCart(int customerId);
    Task<Cart?> GetCartByCustomerId(int customerId);
    Task UpdateCart(Cart cart);
}
