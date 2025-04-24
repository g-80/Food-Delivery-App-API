using System.ComponentModel.DataAnnotations;

public class TokenRefreshRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public required string RefreshToken { get; set; }
}
