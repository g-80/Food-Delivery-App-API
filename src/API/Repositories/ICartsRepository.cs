using Npgsql;

public interface ICartsRepository
{
    Task<int> CreateCart(CreateCartDTO dto);
    Task<Cart?> GetCartByCustomerId(int customerId);
    Task<Cart?> GetCartById(int id);
    Task UpdateCartExpiry(int cartId, DateTime newExpiry);
}
