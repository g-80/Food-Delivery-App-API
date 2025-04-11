public interface IUsersRepository
{
    Task<int> CreateUser(UserDTO dto);
    Task<User?> GetUserById(int id);
    Task<User?> GetUserByPhoneNumber(string phoneNumber);
    Task<bool> UpdateUser(UserDTO dto);
}