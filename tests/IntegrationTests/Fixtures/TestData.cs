/// <summary>
/// Provides test fixtures for integration and unit tests.
/// </summary>
internal static class TestData
{
    public static class FoodPlaces
    {
        public static (double, double) locationLatLong = (51.516061, -0.157185);
        public static List<FoodPlaceCreateRequest> foodPlacesFixtures = new()
        {
            new FoodPlaceCreateRequest
            {
                Name = "First food place",
                Category = "Pizzeria",
                Latitude = 51.5156732,
                Longitude = -0.1522338,
            },
            new FoodPlaceCreateRequest
            {
                Name = "Second food place",
                Category = "Coffee shop",
                Latitude = 51.5162493,
                Longitude = -0.1527786,
            },
            new FoodPlaceCreateRequest
            {
                Name = "Third food place",
                Category = "Greek",
                Latitude = 51.516791,
                Longitude = -0.151466,
            },
        };
        public static readonly List<int> assignedIds = new();
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
                Price = 750,
            },
            new CreateItemRequest
            {
                Name = "Vegetarian Pizza",
                Description = "Very nice vegetarian pizza",
                FoodPlaceId = 1,
                IsAvailable = true,
                Price = 570,
            },
        };

        // Storage for IDs assigned by database
        public static readonly List<int> assignedIds = new();
    }

    public static class Carts
    {
        public static int assignedCartId = -1;
        public static readonly List<RequestedItem> itemRequests = new()
        {
            new() { ItemId = 1, Quantity = 2 },
            new() { ItemId = 2, Quantity = 1 },
        };

        public static List<int> prices = itemRequests
            .Zip(Items.defaults, (itemReq, itemData) => itemData.Price * itemReq.Quantity)
            .ToList();

        public static IEnumerable<CreateCartItemDTO> CreateCartItemDTOs(int cartId = 1)
        {
            return itemRequests.Select(
                (item, i) =>
                    new CreateCartItemDTO
                    {
                        RequestedItem = item,
                        CartId = cartId,
                        UnitPrice = Items.defaults[i].Price,
                        Subtotal = Items.defaults[i].Price * itemRequests[i].Quantity,
                    }
            );
        }

        public static CartPricingDTO CreateCartPricingDTO(int cartId = 1)
        {
            int subtotal = prices.Sum();
            int fees = 75;
            int deliveryFee = 230;
            return new CartPricingDTO
            {
                CartId = cartId,
                Subtotal = subtotal,
                Fees = fees,
                DeliveryFee = deliveryFee,
                Total = subtotal + fees + deliveryFee,
            };
        }
    }

    public static class Orders
    {
        public static CreateOrderDTO CreateOrderDTO(int customerId = 1)
        {
            return new CreateOrderDTO { CustomerId = customerId, TotalPrice = Carts.prices.Sum() };
        }

        public static readonly List<int> assignedIds = new();
    }

    public static class Users
    {
        public static List<UserCreateRequest> createUserRequests = new()
        {
            new()
            {
                FirstName = "Kirov",
                Surname = "Reporting",
                Password = "very_secure_password_123",
                PhoneNumber = "07123456789",
                UserType = UserTypes.customer,
            },
            new()
            {
                FirstName = "John",
                Surname = "Doe",
                Password = "very_secure_password_123",
                PhoneNumber = "07123123123",
                UserType = UserTypes.food_place,
            },
        };

        public static readonly List<UserLoginRequest> loginRequests = createUserRequests
            .Select(req => new UserLoginRequest
            {
                PhoneNumber = req.PhoneNumber,
                Password = req.Password,
            })
            .ToList();

        public static readonly List<int> assignedIds = new();
    }
}
