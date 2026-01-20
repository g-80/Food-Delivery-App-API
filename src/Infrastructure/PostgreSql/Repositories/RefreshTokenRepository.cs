using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    public RefreshTokenRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<RefreshToken?> GetRefreshTokenByUserId(int userId)
    {
        var parameters = new { userId };
        const string sql =
            @"
            SELECT *
            FROM refresh_tokens
            WHERE user_id = @userId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, parameters);
        }
        ;
    }

    public async Task AddRefreshToken(RefreshToken refreshToken)
    {
        var parameters = new
        {
            refreshToken.UserId,
            refreshToken.Token,
            refreshToken.ExpiresAt,
        };
        const string sql =
            @"
            INSERT INTO refresh_tokens
            VALUES
            (@UserId, @Token, @ExpiresAt)
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }

    public async Task DeleteRefreshToken(int userId)
    {
        var parameters = new { userId };
        const string sql =
            @"
            DELETE FROM refresh_tokens
            WHERE user_id = @userId
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(sql, parameters);
        }
        ;
    }
}
