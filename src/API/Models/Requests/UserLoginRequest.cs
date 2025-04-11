using System.ComponentModel.DataAnnotations;

public class UserLoginRequest
{
    [Required]
    [StringLength(11)]
    [RegularExpression("^07[0-9]{9}$")]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    [StringLength(30, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}