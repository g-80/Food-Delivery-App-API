
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton(_ => new FoodPlacesRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new ItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new QuotesRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new QuotesItemsRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(_ => new QuoteTokenService(builder.Configuration["SecretKeys:QuoteToken"]));
builder.Services.AddSingleton<PricingService>();
builder.Services.AddSingleton(_ => new DatabaseInitializer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await dbInitializer.InitializeAsync();

app.Run();
