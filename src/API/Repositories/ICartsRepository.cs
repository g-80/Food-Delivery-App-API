using Npgsql;

public interface ICartsRepository
{
    Task<int> CreateCart(CreateCartDTO dto, NpgsqlTransaction? transaction = null);
    Task<Cart?> GetCartByCustomerId(int customerId);
    Task<Cart?> GetCartById(int id);
}
