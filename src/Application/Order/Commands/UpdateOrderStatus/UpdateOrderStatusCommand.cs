using System.ComponentModel.DataAnnotations;

public class UpdateOrderStatusCommand
{
    [Required]
    [EnumDataType(typeof(OrderStatuses))]
    public required OrderStatuses Status { get; init; }
}
