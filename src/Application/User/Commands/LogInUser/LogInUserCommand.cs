using System.ComponentModel.DataAnnotations;

public class LogInUserCommand
{
    [Required]
    [StringLength(11)]
    [RegularExpression("^07[0-9]{9}$")]
    public required string PhoneNumber { get; init; }

    [Required]
    [StringLength(30, MinimumLength = 8)]
    public required string Password { get; init; }
}
