using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton(_ => new FoodPlacesRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IItemsRepository>(_ => new ItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new CartsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<ICartItemsRepository>(_ => new CartItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new CartPricingsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<PricingService>();
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton(_ => new OrdersRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new OrdersItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<IUsersRepository>(_ => new UsersRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IRefreshTokensRepository>(_ => new RefreshTokensRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddTransient(_ => new UnitOfWork(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new DatabaseInitializer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuerSigningKey = true
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
