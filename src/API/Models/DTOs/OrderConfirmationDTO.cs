public class OrderConfirmationDTO
{
    public required int OrderId { get; init; }
    public required string CustomerName { get; init; }
    public required List<OrderConfirmationItemDTO> OrderItems { get; init; }
}
