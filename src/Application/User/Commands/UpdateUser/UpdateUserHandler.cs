using Microsoft.AspNetCore.Identity;

public class UpdateUserHandler
{
    private readonly IUserRepository _userRepository;

    public UpdateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(UpdateUserCommand request, int userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        var passwordHasher = new PasswordHasher<User>();
        user.FirstName = request.FirstName;
        user.Surname = request.Surname;
        user.PhoneNumber = request.PhoneNumber;
        user.Password = passwordHasher.HashPassword(user, request.Password);

        await _userRepository.UpdateUser(user);
    }
}
