using Hangfire;
using Hangfire.PostgreSql;

namespace FoodDeliveryAppAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            string connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Could not read the database ConnectionString");
            builder.Services.Configure<DatabaseOptions>(options =>
                options.ConnectionString = connectionString
            );

            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IDriverRepository, DriverRepository>();
            builder.Services.AddScoped<IFoodPlaceRepository, FoodPlaceRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddHangfire(configuration =>
                configuration.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString))
            );
            builder.Services.AddHangfireServer();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddTransient<IDatabaseInitialiser, DatabaseInitialiser>();
        }
    }
}
