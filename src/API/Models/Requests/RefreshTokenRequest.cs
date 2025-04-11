using System.ComponentModel.DataAnnotations;

public class RefreshTokenRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public required string RefreshToken { get; set; }
}