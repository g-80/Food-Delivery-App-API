public class DeliveryOfferDTO
{
    public required string FoodPlaceName { get; init; }
    public required Address FoodPlaceAddress { get; init; }
    public required int EstimatedOrderPreparationTime { get; init; }
    public required int EstimatedPickupTime { get; init; }
    public required double TotalDistance { get; init; }
    public required int TotalEstimatedTime { get; init; }
    public required int PaymentAmount { get; init; }
    public required object RouteDataJson { get; init; }
}
