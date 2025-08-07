using Microsoft.AspNetCore.Identity;

public class LogInUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LogInUserHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthTokenResponse?> Handle(LogInUserCommand req)
    {
        var user = await _userRepository.GetUserByPhoneNumber(req.PhoneNumber);
        if (user == null)
        {
            return null;
        }

        var passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(
            user,
            user.Password,
            req.Password
        );
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await _tokenService.GenerateTokens(user.Id.ToString(), user.UserType.ToString());
    }
}
