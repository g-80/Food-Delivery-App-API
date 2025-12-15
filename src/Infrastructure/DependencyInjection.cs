namespace FoodDeliveryAppAPI.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        string connectionString =
            builder.Configuration.GetConnectionString("Postgres")
            ?? throw new Exception("Could not read the database ConnectionString");
        builder.Services.Configure<DatabaseOptions>(options =>
            options.ConnectionString = connectionString
        );

        string redisConnetionString =
            builder.Configuration.GetConnectionString("Redis")
            ?? throw new Exception("Could not read redis connection string");
        builder.Services.Configure<RedisOptions>(options =>
            options.ConnectionString = redisConnetionString
        );
        builder.Services.AddSingleton<RedisConnectionFactory>();

        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<IDriverRepository, DriverRepository>();
        builder.Services.AddScoped<IFoodPlaceRepository, FoodPlaceRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPaymentService, StripePaymentService>();

        builder.Services.AddHttpClient<MapboxJourneyCalculationService>();
        builder.Services.AddScoped<IJourneyCalculationService, MapboxJourneyCalculationService>();

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "[HH:mm:ss] ";
        });

        builder.Services.AddTransient<IDatabaseInitialiser, DatabaseInitialiser>();
    }
}
