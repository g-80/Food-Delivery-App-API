public interface IAddressRepository
{
    Task<Address?> GetAddressById(int id);
    Task<int> AddAddress(Address address, int userId);
}
