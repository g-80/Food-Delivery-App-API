public class OrderConfirmationDTO
{
    public required int OrderId { get; init; }
    public required string CustomerName { get; init; }
    public required IEnumerable<OrderConfirmationItemDTO> OrderItems { get; init; }
}
