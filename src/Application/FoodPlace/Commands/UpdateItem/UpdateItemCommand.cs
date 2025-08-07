using System.ComponentModel.DataAnnotations;

public class UpdateItemCommand : ItemCommand
{
    [Required]
    public required int Id { get; set; }
}
