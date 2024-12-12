using System.ComponentModel.DataAnnotations;

public class SearchFoodPlacesRequest
{
    private string _searchQuery = string.Empty;

    [Required]
    [Range(49.0, 59.0, ErrorMessage = "Invalid location")]
    public double Latitude { get; set; }

    [Required]
    [Range(-8.0, 2.0, ErrorMessage = "Invalid location")]
    public double Longitude { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string SearchQuery
    {
        get => _searchQuery;
        set => _searchQuery = value?.Trim();
    }

}