using System.ComponentModel.DataAnnotations;

public class FoodPlacesNearbyRequest
{
    [Required]
    [Range(49.0, 59.0, ErrorMessage = "Invalid location")]
    public double Latitude { get; set; }

    [Required]
    [Range(-8.0, 2.0, ErrorMessage = "Invalid location")]
    public double Longitude { get; set; }
}
