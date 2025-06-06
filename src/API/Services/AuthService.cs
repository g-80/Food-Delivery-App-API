using System.Transactions;
using Microsoft.AspNetCore.Identity;

public class AuthService
{
    IUsersRepository _usersRepo;
    TokenService _tokenService;
    ICartService _cartService;
    AddressesService _addressesService;

    public AuthService(
        IUsersRepository usersRepository,
        TokenService tokenService,
        ICartService cartService,
        AddressesService addressesService
    )
    {
        _usersRepo = usersRepository;
        _tokenService = tokenService;
        _cartService = cartService;
        _addressesService = addressesService;
    }

    public async Task<int?> SignUpUserAsync(UserCreateRequest request)
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

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var userId = await _usersRepo.CreateUser(userDTO);
        await _addressesService.CreateAddress(request.Address, userId);
        await _cartService.CreateCartAsync(userId);
        scope.Complete();

        return userId;
    }

    public async Task<TokenResponse?> LoginAsync(UserLoginRequest request)
    {
        var user = await _usersRepo.GetUserByPhoneNumber(request.PhoneNumber);
        if (user == null)
        {
            return null;
        }
        if (
            new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, request.Password)
            == PasswordVerificationResult.Failed
        )
        {
            return null;
        }

        return await _tokenService.CreateTokenResponse(user);
    }
}
