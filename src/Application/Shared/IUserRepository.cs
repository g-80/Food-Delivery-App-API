public interface IUserRepository
{
    Task<int> AddUser(User user);
    Task<User?> GetUserById(int id);
    Task<User?> GetUserByPhoneNumber(string phoneNumber);
    Task<bool> UpdateUser(User user);
}
