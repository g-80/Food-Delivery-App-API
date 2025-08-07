public class User
{
    public int Id { get; init; }
    public required string FirstName { get; set; }
    public required string Surname { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public required UserTypes UserType { get; init; }
}
