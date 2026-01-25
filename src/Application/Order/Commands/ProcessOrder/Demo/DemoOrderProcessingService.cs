using Microsoft.AspNetCore.SignalR;

public class DemoOrderProcessingService : IDemoOrderProcessingService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IJourneyCalculationService _journeyCalculationService;
    private readonly ICustomerConnections _customerConnections;
    private readonly IHubContext<CustomerHub> _customerHubContext;
    private readonly IRouteSimulationService _routeSimulationService;
    private readonly ILogger<DemoOrderProcessingService> _logger;
    private readonly IPaymentService _paymentService;
    private const int DEMO_DRIVER_ID = 3;
    private const double DRIVER_SPEED_MPH = 20.0;

    private static readonly TimeSpan ORDER_CONFIRMATION_DELAY = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PREPARING_TO_DELIVERING_DELAY = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan CUSTOMER_CONNECTION_TIMEOUT = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan LOCATION_UPDATE_INTERVAL = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan POST_ARRIVAL_DELAY = TimeSpan.FromSeconds(60);

    public DemoOrderProcessingService(
        IOrderRepository orderRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository,
        IDriverRepository driverRepository,
        IJourneyCalculationService journeyCalculationService,
        ICustomerConnections customerConnections,
        IHubContext<CustomerHub> customerHubContext,
        IRouteSimulationService routeSimulationService,
        ILogger<DemoOrderProcessingService> logger,
        IPaymentService paymentService
    )
    {
        _orderRepository = orderRepository;
        _foodPlaceRepository = foodPlaceRepository;
        _addressRepository = addressRepository;
        _driverRepository = driverRepository;
        _journeyCalculationService = journeyCalculationService;
        _customerConnections = customerConnections;
        _customerHubContext = customerHubContext;
        _routeSimulationService = routeSimulationService;
        _logger = logger;
        _paymentService = paymentService;
    }

    public async Task ProcessDemoOrder(Order order)
    {
        _logger.LogInformation(
            "Starting demo order processing for order ID: {OrderId}",
            order.Id
        );

        try
        {
            await AutoConfirmOrder(order);

            await SimulateDriverAssignment(order);

            await Task.Delay(PREPARING_TO_DELIVERING_DELAY);
            await TransitionToDelivering(order);

            var customerConnected = await WaitForCustomerConnection(order.CustomerId);
            if (!customerConnected)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} did not connect within timeout for order {OrderId}. Continuing anyway.",
                    order.CustomerId,
                    order.Id
                );
            }

            await SendDeliveryConfirmationCode(order);

            await SimulateDeliveryRoute(order);

            await Task.Delay(POST_ARRIVAL_DELAY);
            await CompleteOrder(order);

            _logger.LogInformation(
                "Demo order processing completed for order ID: {OrderId}",
                order.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during demo order processing for order ID: {OrderId}",
                order.Id
            );
            throw;
        }
    }

    private async Task AutoConfirmOrder(Order order)
    {
        _logger.LogInformation(
            "Auto-confirming order {OrderId} after {Delay}s delay",
            order.Id,
            ORDER_CONFIRMATION_DELAY.TotalSeconds
        );

        await Task.Delay(ORDER_CONFIRMATION_DELAY);

        order.Status = OrderStatuses.preparing;
        await _orderRepository.UpdateOrderStatus(order);

        order.CreateDelivery();
        await _orderRepository.AddDelivery(order.Id, order.Delivery!);

        _logger.LogInformation("Order {OrderId} auto-confirmed and set to preparing", order.Id);
    }

    private async Task SimulateDriverAssignment(Order order)
    {
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(order.FoodPlaceId);
        if (foodPlace == null)
        {
            throw new InvalidOperationException($"Food place {order.FoodPlaceId} not found");
        }

        await _driverRepository.ConnectDriver(DEMO_DRIVER_ID, DriverStatuses.online);

        var demoDriver = new Driver
        {
            Id = DEMO_DRIVER_ID,
            Status = DriverStatuses.online,
            Location = foodPlace.Location,
        };
        await _driverRepository.UpsertDriverLocation(demoDriver);

        _logger.LogInformation(
            "Demo driver {DriverId} connected at food place location for order {OrderId}",
            DEMO_DRIVER_ID,
            order.Id
        );

        var routeJson = await CalculateRoute(order, foodPlace);

        demoDriver.Status = DriverStatuses.delivering;
        await _driverRepository.UpdateDriverStatus(demoDriver);

        order.Delivery!.DriverId = DEMO_DRIVER_ID;
        order.Delivery.Status = DeliveryStatuses.pickup;
        order.Delivery.RouteJson = routeJson;

        await _orderRepository.UpdateDelivery(order.Id, order.Delivery);

        _logger.LogInformation(
            "Demo driver {DriverId} assigned to order {OrderId}",
            DEMO_DRIVER_ID,
            order.Id
        );
    }

    private async Task<string> CalculateRoute(Order order, FoodPlace foodPlace)
    {
        var deliveryAddress =
            await _addressRepository.GetAddressById(order.DeliveryAddressId)
            ?? throw new InvalidOperationException("Delivery address not found");

        var addressString =
            $"{deliveryAddress.NumberAndStreet}, {deliveryAddress.City}, {deliveryAddress.Postcode}";
        var customerLocation =
            await _journeyCalculationService.GeocodeAddressAsync(addressString)
            ?? throw new InvalidOperationException(
                $"Could not geocode customer address: {addressString}"
            );

        var (_, routeJson) = await _journeyCalculationService.CalculateRouteAsync(
            new Location[] { foodPlace.Location, customerLocation }
        );

        return routeJson;
    }

    private async Task TransitionToDelivering(Order order)
    {
        order.Status = OrderStatuses.delivering;
        order.Delivery!.Status = DeliveryStatuses.delivering;

        await _orderRepository.UpdateOrderStatus(order);
        await _orderRepository.UpdateDelivery(order.Id, order.Delivery);

        _logger.LogInformation("Order {OrderId} transitioned to delivering status", order.Id);
    }

    private async Task<bool> WaitForCustomerConnection(int customerId)
    {
        _logger.LogInformation(
            "Waiting for customer {CustomerId} to connect (timeout: {Timeout}s)",
            customerId,
            CUSTOMER_CONNECTION_TIMEOUT.TotalSeconds
        );

        return await _customerConnections.WaitForCustomerConnection(
            customerId,
            CUSTOMER_CONNECTION_TIMEOUT
        );
    }

    private async Task SendDeliveryConfirmationCode(Order order)
    {
        var confirmationCode = order.Delivery!.ConfirmationCode;

        await _customerHubContext
            .Clients.User(order.CustomerId.ToString())
            .SendAsync(
                "ReceiveConfirmationCode",
                new { OrderId = order.Id, ConfirmationCode = confirmationCode }
            );

        _logger.LogInformation(
            "Sent confirmation code to customer {CustomerId} for order {OrderId}",
            order.CustomerId,
            order.Id
        );
    }

    private async Task SimulateDeliveryRoute(Order order)
    {
        if (string.IsNullOrEmpty(order.Delivery?.RouteJson))
        {
            _logger.LogWarning("No route data available for order {OrderId}", order.Id);
            return;
        }

        var coordinates = _routeSimulationService.ExtractCoordinates(order.Delivery.RouteJson);
        if (!coordinates.Any())
        {
            _logger.LogWarning("No coordinates found in route for order {OrderId}", order.Id);
            return;
        }

        var simulatedPositions = _routeSimulationService.CalculatePositionsAtSpeed(
            coordinates,
            DRIVER_SPEED_MPH,
            LOCATION_UPDATE_INTERVAL
        );

        _logger.LogInformation(
            "Starting route simulation for order {OrderId} with {PositionCount} positions",
            order.Id,
            simulatedPositions.Count()
        );

        foreach (var position in simulatedPositions)
        {
            await _customerHubContext
                .Clients.User(order.CustomerId.ToString())
                .SendAsync(
                    "DriverLocationUpdate",
                    new
                    {
                        OrderId = order.Id,
                        Latitude = position.Latitude,
                        Longitude = position.Longitude,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    }
                );

            await Task.Delay(LOCATION_UPDATE_INTERVAL);
        }

        _logger.LogInformation("Route simulation completed for order {OrderId}", order.Id);
    }

    private async Task CompleteOrder(Order order)
    {
        order.Status = OrderStatuses.completed;
        order.Delivery!.Status = DeliveryStatuses.delivered;
        order.Delivery.DeliveredAt = DateTime.UtcNow;

        await _orderRepository.UpdateOrderStatus(order);
        await _orderRepository.UpdateDelivery(order.Id, order.Delivery);

        await _customerHubContext
            .Clients.User(order.CustomerId.ToString())
            .SendAsync(
                "DeliveryCompleted",
                new { OrderId = order.Id, CompletedAt = order.Delivery.DeliveredAt }
            );

        await _driverRepository.DisconnectDriver(DEMO_DRIVER_ID);

        _logger.LogInformation("Order {OrderId} completed", order.Id);
    }
}
