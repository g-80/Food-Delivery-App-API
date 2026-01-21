public static class OrderTestsHelper
{
    public static Order CreateTestOrder(int id = 1)
    {
        return new Order
        {
            Id = id,
            CustomerId = 1,
            FoodPlaceId = 1,
            DeliveryAddressId = 2,
            Items = CreateTestOrderItems(),
            ServiceFee = 0,
            DeliveryFee = 200,
            Delivery = new Delivery
            {
                Id = 1,
                ConfirmationCode = "Testing",
                Status = DeliveryStatuses.assigningDriver,
            },
            Payment = new Payment
            {
                Amount = 1400,
                Status = PaymentStatuses.NotConfirmed,
                StripePaymentIntentId = "pi_test123",
            },
            Status = OrderStatuses.pendingConfirmation,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public static FoodPlace CreateTestFoodPlace()
    {
        return new FoodPlace
        {
            Id = 1,
            Name = "Test Restaurant",
            Description = "A place for testing",
            Category = "Test Category",
            AddressId = 1,
            Location = new Location { Latitude = 51.505, Longitude = -0.3099 },
        };
    }

    public static List<FoodPlaceItem> CreateTestFoodPlaceItems()
    {
        var items = new List<FoodPlaceItem>();
        for (int i = 1; i <= 3; i++)
        {
            items.Add(
                new FoodPlaceItem()
                {
                    Id = i,
                    Name = $"food-place-item-{i}",
                    Price = 400,
                    IsAvailable = true,
                }
            );
        }
        return items;
    }

    public static List<OrderItem> CreateTestOrderItems()
    {
        var items = new List<OrderItem>();
        for (int i = 1; i <= 3; i++)
        {
            items.Add(
                new OrderItem()
                {
                    ItemId = i,
                    Quantity = 1,
                    UnitPrice = 400,
                }
            );
        }
        return items;
    }

    public static List<AvailableDriver> CreateTestAvailableDrivers(int count)
    {
        var drivers = new List<AvailableDriver>();
        for (int i = 1; i <= count; i++)
        {
            drivers.Add(CreateTestAvailableDriver(i));
        }
        return drivers;
    }

    public static AvailableDriver CreateTestAvailableDriver(int id)
    {
        return new AvailableDriver
        {
            Id = id,
            Status = DriverStatuses.online,
            Distance = 500.0,
            Location = CreateTestLocation(),
        };
    }

    public static DeliveryAssignmentJob CreateTestDeliveryAssignmentJob(
        int orderId,
        int offeredDriverId = 0,
        bool shouldCreateCts = true
    )
    {
        return new DeliveryAssignmentJob
        {
            OrderId = orderId,
            OfferedDriverId = offeredDriverId,
            Cts = shouldCreateCts ? new CancellationTokenSource() : null,
        };
    }

    public static List<Address> CreateTestAddresses()
    {
        return new List<Address>
        {
            new Address
            {
                NumberAndStreet = "123 Food St",
                City = "Test City",
                Postcode = "W8 8BB",
            },
            new Address
            {
                NumberAndStreet = "456 Customer Avenue",
                City = "Test City",
                Postcode = "W18 18BB",
            },
        };
    }

    public static User CreateTestUser()
    {
        return new User
        {
            Id = 1,
            FirstName = "Test",
            Surname = "User",
            PhoneNumber = "07123456789",
            Password = "hashed_password",
            UserType = UserTypes.customer,
        };
    }

    public static (MapboxRouteInfo, string) CreateTestMapboxRoute(
        double distance = 1000.0,
        double duration = 600.0
    )
    {
        var routeInfo = new MapboxRouteInfo
        {
            Distance = 5000.0,
            Duration = 600.0,
            Legs = new[]
            {
                new MapboxLegInfo { Distance = 5000.0, Duration = 600.0 },
            },
        };
        var routeJson = "{}";
        return (routeInfo, routeJson);
    }

    public static Location CreateTestLocation(double latitude = 51.5074, double longitude = -0.1278)
    {
        return new Location { Latitude = latitude, Longitude = longitude };
    }
}
