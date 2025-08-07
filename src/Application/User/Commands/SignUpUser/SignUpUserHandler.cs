using System.Transactions;
using Microsoft.AspNetCore.Identity;

public class SignUpUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICartRepository _cartRepository;

    public SignUpUserHandler(
        IUserRepository userRepository,
        IAddressRepository addressRepository,
        ICartRepository cartRepository
    )
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
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

        return userId;
    }
}
