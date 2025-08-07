public class GetUserHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserDTO> Handle(int userId)
    {
        var user = await _userRepository.GetUserById(userId);

        return new GetUserDTO
        {
            FirstName = user!.FirstName,
            Surname = user.Surname,
            PhoneNumber = user.PhoneNumber,
        };
    }
}
