using Dapper;
using Npgsql;

public class RefreshTokensRepository : BaseRepo, IRefreshTokensRepository
{
    public RefreshTokensRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<RefreshToken?> GetRefreshTokenByUserId(int userId)
    {
        var parameters = new { userId };
        const string sql = @"
            SELECT *
            FROM refresh_tokens
            WHERE user_id = @userId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, parameters);
        }
        ;
    }

    public async Task CreateRefreshToken(RefreshTokenDTO dto)
    {
        var parameters = dto;
        const string sql = @"
            INSERT INTO refresh_tokens
            VALUES
            (@UserId, @Token, @ExpiresAt)
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task DeleteRefreshToken(int userId)
    {
        var parameters = new { userId };
        const string sql = @"
            DELETE FROM refresh_tokens
            WHERE user_id = @userId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}