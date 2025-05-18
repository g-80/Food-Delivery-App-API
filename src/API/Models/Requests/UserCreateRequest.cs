using System.ComponentModel.DataAnnotations;

public class UserCreateRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public required string Surname { get; init; }

    [Required]
    [StringLength(11)]
    [RegularExpression("^07[0-9]{9}$")]
    public required string PhoneNumber { get; init; }

    [Required]
    [StringLength(30, MinimumLength = 8)]
    public required string Password { get; init; }

    public required AddressCreateRequest Address { get; init; }

    [Required]
    [EnumDataType(typeof(UserTypes))]
    public required UserTypes UserType { get; init; }
}
