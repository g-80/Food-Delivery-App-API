/// <summary>
/// Provides test fixtures for integration and unit tests.
/// </summary>
internal static class TestData
{
    public static class FoodPlaces
    {
        public static (double, double) locationLatLong = (51.516061, -0.157185);
        public static List<FoodPlace> foodPlacesFixtures = new List<FoodPlace>
        {
            new FoodPlace
            {
                Name = "First food place",
                Category = "Pizzeria",
                Latitude = 51.5156732,
                Longitude = -0.1522338,
                CreatedAt = DateTime.UtcNow
            },
            new FoodPlace
            {
                Name = "Second food place",
                Category = "Coffee shop",
                Latitude = 51.5162493,
                Longitude = -0.1527786,
                CreatedAt = DateTime.UtcNow
            },
            new FoodPlace
            {
                Name = "Third food place",
                Category = "Greek",
                Latitude = 51.516791,
                Longitude = -0.151466,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public static class Items
    {
        public static readonly List<CreateItemRequest> defaults = new()
        {
            new CreateItemRequest
            {
                Name = "Amazing Pizza",
                Description = "Very nice pizza",
                FoodPlaceId = 1,
                IsAvailable = true,
                Price = 750
            },
            new CreateItemRequest
            {
                Name = "Vegetarian Pizza",
                Description = "Very nice vegetarian pizza",
                FoodPlaceId = 1,
                IsAvailable = true,
                Price = 570
            }
        };

        // Storage for IDs assigned by database
        public static readonly List<int> assignedIds = new();
    }

    public static class Orders
    {
        public static readonly List<RequestedItem> itemRequests = new()
        {
            new() { ItemId = 1, Quantity = 2 },
            new() { ItemId = 2, Quantity = 1 }
        };

        public static List<int> prices = new List<int>
        {
            Items.defaults[0].Price * itemRequests[0].Quantity,
            Items.defaults[1].Price * itemRequests[1].Quantity
        };

        public static CreateQuoteDTO CreateQuoteDTO(int customerId = 1)
        {
            return new CreateQuoteDTO
            {
                CustomerId = customerId,
                TotalPrice = prices.Sum(),
                Expiry = DateTime.UtcNow.AddMinutes(5)
            };
        }

        public static CreateOrderDTO CreateOrderDTO(int customerId = 1)
        {
            return new CreateOrderDTO
            {
                CustomerId = customerId,
                TotalPrice = prices.Sum()
            };
        }
    }
}