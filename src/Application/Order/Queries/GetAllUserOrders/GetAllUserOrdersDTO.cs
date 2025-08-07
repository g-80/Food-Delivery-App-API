public class UserOrdersSummaryDTO
{
    public required int OrderId { get; init; }
    public required string FoodPlaceName { get; init; }
    public required OrderStatuses Status { get; init; }
    public required int ItemsCount { get; init; }
    public required int Total { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public class GetAllUserOrdersDTO
{
    public required IEnumerable<UserOrdersSummaryDTO> Orders { get; init; }
}
