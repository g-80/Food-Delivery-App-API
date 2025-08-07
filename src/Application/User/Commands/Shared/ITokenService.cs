public interface ITokenService
{
    Task<AuthTokenResponse> GenerateTokens(string userId, string userType);
    Task<AuthTokenResponse?> RenewAccessToken(RenewAccessTokenCommand request);
}
