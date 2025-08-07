using System.ComponentModel.DataAnnotations;

public class AddressCreateRequest
{
    [Required]
    [StringLength(40, MinimumLength = 9)]
    public required string NumberAndStreet { get; init; }

    [Required]
    [StringLength(30, MinimumLength = 5)]
    public required string City { get; init; }

    [Required]
    [RegularExpression("^([A-Z][A-HJ-Y]?[0-9][A-Z0-9]? ?[0-9][0-9]?[A-Z]{2})$")]
    public required string Postcode { get; init; }
}
