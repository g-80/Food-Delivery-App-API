public class User
{
    public required int Id { get; init; }
    public required string FirstName { get; init; }
    public required string Surname { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Password { get; init; }
    public required UserTypes UserType { get; init; }
    public required DateTime CreatedAt { get; init; }
}
