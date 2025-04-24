using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Could not read the database ConnectionString");
builder.Services.AddSingleton(connectionString);
builder.Services.AddScoped<IFoodPlacesRepository>(_ => new FoodPlacesRepository(
    connectionString,
    builder.Configuration.GetValue<int>("SearchDistance:Default")
));
builder.Services.AddScoped<IFoodPlacesService, FoodPlacesService>();
builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICartsRepository, CartsRepository>();
builder.Services.AddScoped<ICartItemsRepository, CartItemsRepository>();
builder.Services.AddScoped<ICartPricingsRepository, CartPricingsRepository>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IOrdersItemsRepository, OrdersItemsRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
builder.Services.AddTransient<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddTransient<DatabaseInitializer>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
dbInitializer.InitializeDatabase();

app.Run();
