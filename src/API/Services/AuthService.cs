using Microsoft.AspNetCore.Identity;

public class AuthService
{
    IUsersRepository _usersRepo;
    TokenService _tokenService;

    public AuthService(IUsersRepository usersRepository, TokenService tokenService)
    {
        _usersRepo = usersRepository;
        _tokenService = tokenService;
    }

    public async Task<UserDTO?> RegisterUserAsync(CreateUserRequest request)
    {
        if ((await _usersRepo.GetUserByPhoneNumber(request.PhoneNumber)) != null)
        {
            return null;
        }

        var userDTO = new UserDTO();
        var hashedPassword = new PasswordHasher<UserDTO>().HashPassword(userDTO, request.Password);

        userDTO.FirstName = request.FirstName;
        userDTO.Surname = request.Surname;
        userDTO.PhoneNumber = request.PhoneNumber;
        userDTO.Password = hashedPassword;
        userDTO.UserType = request.UserType;

        userDTO.Id = await _usersRepo.CreateUser(userDTO);

        return userDTO;
    }

    public async Task<TokenResponse?> LoginAsync(UserLoginRequest request)
    {
        var user = await _usersRepo.GetUserByPhoneNumber(request.PhoneNumber);
        if (user == null)
        {
            return null;
        }
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await _tokenService.CreateTokenResponse(user);
    }
}
