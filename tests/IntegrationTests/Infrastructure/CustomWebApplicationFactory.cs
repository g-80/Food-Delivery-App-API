using IntegrationTests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IntegrationTestFixture _fixture;

    public CustomWebApplicationFactory(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Postgres",
            _fixture.PostgresConnectionString
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Redis",
            _fixture.RedisConnectionString
        );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IJourneyCalculationService>();
            services.AddScoped<IJourneyCalculationService, MockMapboxService>();

            services.RemoveAll<IDeliveryAssignmentService>();
            services.AddScoped<IDeliveryAssignmentService>(
                provider => new DeliveryAssignmentService(
                    provider.GetRequiredService<IDriverRepository>(),
                    provider.GetRequiredService<IFoodPlaceRepository>(),
                    provider.GetRequiredService<IAddressRepository>(),
                    provider.GetRequiredService<IJourneyCalculationService>(),
                    provider.GetRequiredService<IOrderRepository>(),
                    provider.GetRequiredService<IHubContext<DriverHub>>(),
                    provider.GetRequiredService<IDeliveriesAssignments>(),
                    provider.GetRequiredService<IDriverPaymentService>(),
                    provider.GetRequiredService<ILogger<DeliveryAssignmentService>>(),
                    offerTimeout: TimeSpan.FromMilliseconds(750),
                    retryInterval: TimeSpan.FromMilliseconds(500)
                )
            );
        });

        builder.UseEnvironment("Test");
    }
}
