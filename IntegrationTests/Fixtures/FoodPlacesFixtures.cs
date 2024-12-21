internal static class FoodPlacesFixtures
{
    public static (double, double) locationLatLong = (51.516061, -0.157185);
    public static List<FoodPlace> GetFoodPlacesFixtures() => new List<FoodPlace>
    {
        new FoodPlace { Name = "First food place", Category = "Turkish", Latitude = 51.5156732, Longitude = -0.1522338},
        new FoodPlace { Name = "Second food place", Category = "Coffee shop", Latitude = 51.5162493, Longitude = -0.1527786},
        new FoodPlace { Name = "Third food place", Category = "Polish", Latitude = 51.516791, Longitude = -0.151466}
    };
}