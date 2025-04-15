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
builder.Services.AddSingleton<IFoodPlacesRepository>(_ => new FoodPlacesRepository(
    connectionString,
    builder.Configuration.GetValue<int>("SearchDistance:Default")
));
builder.Services.AddSingleton<IFoodPlacesService, FoodPlacesService>();
builder.Services.AddSingleton<IItemsRepository>(_ => new ItemsRepository(connectionString));
builder.Services.AddSingleton<ICartsRepository>(_ => new CartsRepository(connectionString));
builder.Services.AddSingleton<ICartItemsRepository>(_ => new CartItemsRepository(connectionString));
builder.Services.AddSingleton<ICartPricingsRepository>(_ => new CartPricingsRepository(
    connectionString
));
builder.Services.AddSingleton<IPricingService, PricingService>();
builder.Services.AddSingleton<ICartService, CartService>();
builder.Services.AddSingleton<IOrdersRepository>(_ => new OrdersRepository(connectionString));
builder.Services.AddSingleton<IOrdersItemsRepository>(_ => new OrdersItemsRepository(
    connectionString
));
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IUsersRepository>(_ => new UsersRepository(connectionString));
builder.Services.AddSingleton<IRefreshTokensRepository>(_ => new RefreshTokensRepository(
    connectionString
));
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddTransient(_ => new UnitOfWork(connectionString));
builder.Services.AddSingleton(_ => new DatabaseInitializer(connectionString));

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
