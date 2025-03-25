
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton(_ => new FoodPlacesRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new ItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new CartsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new CartItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new CartPricingsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<PricingService>();
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton(_ => new OrdersRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new OrdersItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<OrderService>();
builder.Services.AddTransient(_ => new UnitOfWork(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new DatabaseInitializer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await dbInitializer.InitializeAsync();

app.Run();
