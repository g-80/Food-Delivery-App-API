public interface IRefreshTokensRepository
{
    Task CreateRefreshToken(RefreshToken refreshToken);
    Task DeleteRefreshToken(int userId);
    Task<RefreshToken?> GetRefreshTokenByUserId(int userId);
}
