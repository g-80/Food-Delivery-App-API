internal static class Fixtures
{
    public static (double, double) locationLatLong = (51.516061, -0.157185);
    public static List<FoodPlace> foodPlacesFixtures = new List<FoodPlace>
    {
        new FoodPlace { Name = "First food place", Category = "Pizzeria", Latitude = 51.5156732, Longitude = -0.1522338, CreatedAt = new DateTime()},
        new FoodPlace { Name = "Second food place", Category = "Coffee shop", Latitude = 51.5162493, Longitude = -0.1527786, CreatedAt = new DateTime()},
        new FoodPlace { Name = "Third food place", Category = "Greek", Latitude = 51.516791, Longitude = -0.151466, CreatedAt = new DateTime()}
    };
    public static List<CreateItemRequest> itemsFixtures = new List<CreateItemRequest>
    {
        new CreateItemRequest { Name = "Amazing Pizza", Description = "Very nice pizza", FoodPlaceId = 1, IsAvailable = true, Price = 750},
        new CreateItemRequest { Name = "Vegetarian Pizza", Description = "Very nice vegetarian pizza", FoodPlaceId = 1, IsAvailable = true, Price = 570},
    };
    // for future use of uuids generated from db
    public static List<int> itemsFixturesIds = new List<int>();
    public static List<ItemRequest> itemRequests = new List<ItemRequest>
    {
        new() { ItemId = 1, Quantity = 2 },
        new() { ItemId = 2, Quantity = 1 }
    };
}