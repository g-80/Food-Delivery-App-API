namespace IntegrationTests.Helpers;

public static class Consts
{
    public const double LondonCentralLat = 51.5074;
    public const double LondonCentralLong = -0.1278;
    public const double GlasgowLat = 55.8624;
    public const double GlasgowLong = -4.2564;

    public const string TestPassword = "TestPassword123";

    public const string CustomerPhoneNumber = "07123456789";
    public const string FoodPlacePhoneNumber = "07123123123";
    public const string DriverPhoneNumber = "07321321321";

    public static class Addresses
    {
        public const string Street = "123 Test Street";
        public const string City = "London";
        public const string Postcode = "SW1A 1AA";
    }

    public static class Prices
    {
        public const int DefaultItemPrice = 350;
        public const int ServiceFee = 120;
        public const int DeliveryFee = 300;
    }

    public static class Quantities
    {
        public const int Default = 2;
    }

    public static class Urls
    {
        public const string login = "/api/auth/login";
        public const string signup = "/api/auth/signup";
        public const string foodPlaces = "/api/food-places/";
        public const string foodPlacesItems = "/api/food-places/items";
        public const string foodPlacesNearby = "/api/food-places/nearby";
        public const string foodPlacesSearch = "/api/food-places/search";
        public const string carts = "/api/carts/";
        public const string orders = "/api/orders/";
        public const string ordersCancel = "/api/orders/cancel/";
        public const string ordersConfirm = "/api/orders/confirmation/";
        public const string ordersStatus = "/api/orders/status/";
        public const string stripeWebhook = "/api/orders/webhook/stripe";
    }
}
