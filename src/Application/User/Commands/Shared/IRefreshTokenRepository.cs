public interface IRefreshTokenRepository
{
    Task AddRefreshToken(RefreshToken refreshToken);
    Task DeleteRefreshToken(int userId);
    Task<RefreshToken?> GetRefreshTokenByUserId(int userId);
}
