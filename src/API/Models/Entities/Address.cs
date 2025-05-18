public class Address
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string NumberAndSteet { get; init; }
    public required string City { get; init; }
    public required string Postcode { get; init; }
    public required bool IsPrimary { get; init; }
    public required DateTime CreatedAt { get; init; }
}
