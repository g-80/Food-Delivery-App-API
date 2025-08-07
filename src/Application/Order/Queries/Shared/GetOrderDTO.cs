public class GetOrderDTO : CheckoutDTO<OrderItemDTO>
{
    public required int OrderId { get; init; }
    public required OrderStatuses Status { get; init; }
    public required Address DeliveryAddress { get; init; }
    public required DateTime CreatedAt { get; init; }
}
