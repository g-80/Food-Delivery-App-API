public class Driver
{
    public required int Id { get; init; }
    public required DriverStatuses Status { get; set; }
    public Location? Location { get; set; }
}
