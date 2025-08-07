using System.ComponentModel.DataAnnotations;

public class SignUpUserCommand : UserInfo
{
    public required AddressCreateRequest Address { get; init; }

    [Required]
    [EnumDataType(typeof(UserTypes))]
    public required UserTypes UserType { get; init; }
}
