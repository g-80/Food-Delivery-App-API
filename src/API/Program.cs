using FoodDeliveryAppAPI.Api;
using FoodDeliveryAppAPI.Application;
using FoodDeliveryAppAPI.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddApiServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapHub<DriverHub>("/hubs/driver");
app.MapHub<FoodPlaceHub>("/hubs/foodplace");
app.MapHub<CustomerHub>("/hubs/customer");

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
var dbInitialiser = app.Services.GetRequiredService<IDatabaseInitialiser>();
dbInitialiser.InitialiseDatabase();

app.Run();
