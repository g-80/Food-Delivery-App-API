using System.ComponentModel.DataAnnotations;

public class OrderStatusUpdateRequest
{
    [Required]
    [EnumDataType(typeof(OrderStatuses))]
    public required OrderStatuses Status { get; init; }
}
