public class CreateAddressDTO
{
    public required int UserId { get; init; }
    public required string NumberAndStreet { get; init; }
    public required string City { get; init; }
    public required string Postcode { get; init; }
    public required bool IsPrimary { get; init; }
}
