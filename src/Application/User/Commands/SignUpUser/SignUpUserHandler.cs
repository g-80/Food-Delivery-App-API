using System.Transactions;
using Microsoft.AspNetCore.Identity;

public class SignUpUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<SignUpUserHandler> _logger;

    public SignUpUserHandler(
        IUserRepository userRepository,
        IAddressRepository addressRepository,
        ICartRepository cartRepository,
        ILogger<SignUpUserHandler> logger
    )
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<int?> Handle(SignUpUserCommand req)
    {
        if ((await _userRepository.GetUserByPhoneNumber(req.PhoneNumber)) != null)
        {
            return null;
        }

        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            FirstName = req.FirstName,
            Surname = req.Surname,
            PhoneNumber = req.PhoneNumber,
            Password = passwordHasher.HashPassword(null!, req.Password),
            UserType = req.UserType,
        };

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var userId = await _userRepository.AddUser(user);
        await _addressRepository.AddAddress(
            new Address
            {
                NumberAndStreet = req.Address.NumberAndStreet,
                City = req.Address.City,
                Postcode = req.Address.Postcode,
            },
            userId
        );
        if (user.UserType == UserTypes.customer)
        {
            await _cartRepository.AddCart(userId);
        }
        scope.Complete();
        _logger.LogInformation(
            "User with ID: {UserId} and phone number {PhoneNumber} signed up successfully as {UserType}",
            userId,
            req.PhoneNumber,
            user.UserType
        );

        return userId;
    }
}
