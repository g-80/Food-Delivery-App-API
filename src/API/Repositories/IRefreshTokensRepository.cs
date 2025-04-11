public interface IRefreshTokensRepository
{
    Task CreateRefreshToken(RefreshTokenDTO dto);
    Task DeleteRefreshToken(int userId);
    Task<RefreshToken?> GetRefreshTokenByUserId(int userId);
}