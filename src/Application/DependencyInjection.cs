namespace FoodDeliveryAppAPI.Application
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<AddItemHandler>();
            builder.Services.AddScoped<RemoveItemHandler>();
            builder.Services.AddScoped<UpdateItemQuantityHandler>();
            builder.Services.AddScoped<GetCartHandler>();

            builder.Services.AddScoped<DisconnectDriverHandler>();
            builder.Services.AddScoped<GoOnlineHandler>();
            builder.Services.AddScoped<UpsertLocationHandler>();

            builder.Services.AddScoped<CreateFoodPlaceHandler>();
            builder.Services.AddScoped<CreateItemHandler>();
            builder.Services.AddScoped<UpdateItemHandler>();
            builder.Services.AddScoped<GetFoodPlaceHandler>();
            builder.Services.AddScoped<GetNearbyFoodPlacesHandler>();
            builder.Services.AddScoped<SearchFoodPlacesHandler>();

            builder.Services.AddScoped<CreateOrderHandler>();
            builder.Services.AddScoped<CancelOrderHandler>();
            builder.Services.AddScoped<UpdateOrderStatusHandler>();
            builder.Services.AddScoped<GetOrderHandler>();
            builder.Services.AddScoped<GetAllUserOrdersHandler>();

            builder.Services.AddScoped<IOrderConfirmationService, OrderConfirmationService>();
            builder.Services.AddSingleton<OrdersConfirmations>();

            builder.Services.AddScoped<IDeliveryAssignmentService, DeliveryAssignmentService>();
            builder.Services.AddSingleton<IDeliveriesAssignments, DeliveriesAssignments>();
            builder.Services.AddTransient<JourneyCalculationService>();

            builder.Services.AddScoped<LogInUserHandler>();
            builder.Services.AddScoped<SignUpUserHandler>();
            builder.Services.AddScoped<RenewAccessTokenHandler>();
            builder.Services.AddTransient<ITokenService, TokenService>();
            builder.Services.AddScoped<UpdateUserHandler>();
            builder.Services.AddScoped<GetUserHandler>();
        }
    }
}
