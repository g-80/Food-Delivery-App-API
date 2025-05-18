public class OrderOfferDTO
{
    // not sure if we need latlong
    public required string FoodPlace { get; init; }
    public required Address FoodPlaceAddress { get; init; }
    public required int DistanceToFoodPlace { get; init; }
    public required TimeSpan EstimatedTimeToFoodPlace { get; init; }
    public required TimeSpan EstimatedOrderPreparationTime { get; init; }
    public required Address DeliveryDestinationAddress { get; init; }
    public required int DistanceToDeliveryDestination { get; init; }
    public required TimeSpan EstimatedTimeToDeliveryDestination { get; init; }
}
