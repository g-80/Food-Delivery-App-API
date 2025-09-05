using System.Collections.Concurrent;

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
            Items = new List<OrderItem>(),
            Subtotal = 1000,
            ServiceFee = 0,
            DeliveryFee = 200,
            Total = 1200,
            Delivery = new Delivery
            {
                Id = 1,
                AddressId = 2,
                ConfirmationCode = "Testing",
                Status = DeliveryStatuses.assigningDriver,
            },
            Payment = new Payment
            {
                Amount = 1200,
                Status = PaymentStatuses.NotConfirmed,
                StripePaymentIntentId = "pi_test123",
            },
            Status = OrderStatuses.preparing,
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
        };
    }

    public static DeliveryAssignmentJob CreateTestDeliveryAssignmentJob(int orderId)
    {
        return new DeliveryAssignmentJob
        {
            OrderId = orderId,
            CurrentAttempt = 0,
            AssignedDriverId = 0,
            PendingOffers = new ConcurrentDictionary<int, CancellationTokenSource>(),
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
}
