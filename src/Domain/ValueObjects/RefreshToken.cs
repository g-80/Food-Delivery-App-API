public class RefreshToken
{
    public required int UserId { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
