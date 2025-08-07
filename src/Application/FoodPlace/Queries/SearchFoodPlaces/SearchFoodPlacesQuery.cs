using System.ComponentModel.DataAnnotations;

public class SearchFoodPlacesQuery : UserLocationQuery
{
    private string _searchQuery = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string SearchQuery
    {
        get => _searchQuery;
        set => _searchQuery = value.Trim();
    }
}
