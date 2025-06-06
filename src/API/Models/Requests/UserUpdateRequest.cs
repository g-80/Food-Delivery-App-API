using System.ComponentModel.DataAnnotations;

public class UserUpdateRequest
{
    [Required]
    public required int Id { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Surname { get; set; }

    [Required]
    [StringLength(11)]
    [RegularExpression("^07[0-9]{9}$")]
    public required string PhoneNumber { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 8)]
    public required string Password { get; set; }
}
