public class RenewAccessTokenHandler
{
    private readonly ITokenService _tokenService;

    public RenewAccessTokenHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<AuthTokenResponse?> Handle(RenewAccessTokenCommand request)
    {
        return await _tokenService.RenewAccessToken(request);
    }
}
