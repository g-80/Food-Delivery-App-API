public class DeliveryOfferDTO
{
    public required string FoodPlaceName { get; init; }
    public required Address FoodPlaceAddress { get; init; }
    public required double DistanceToFoodPlace { get; init; }
    public required TimeSpan EstimatedOrderPreparationTime { get; init; }
    public required TimeSpan EstimatedPickupTime { get; init; }

    public required Address DeliveryDestinationAddress { get; init; }
    public required double DistanceToDeliveryDestination { get; init; }
    public required TimeSpan EstimatedDeliveryTime { get; init; }

    public required double TotalDistance { get; init; }
    public required TimeSpan TotalEstimatedTime { get; init; }
    public required MapboxRoute Route { get; init; }
}
