using System.ComponentModel.DataAnnotations;

public class RenewAccessTokenCommand
{
    [Required]
    public required int UserId { get; init; }

    [Required]
    public required string RefreshToken { get; init; }
}
