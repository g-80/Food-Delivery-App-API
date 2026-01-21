using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

public class DeliveryAssignmentService : IDeliveryAssignmentService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IOrderRepository _orderRepository;

    private readonly IJourneyCalculationService _journeyCalculationService;
    private readonly IDriverPaymentService _driverPaymentService;
    private readonly IDeliveriesAssignments _deliveriesAssignments;

    private readonly IHubContext<DriverHub> _hubContext;
    private readonly ILogger<DeliveryAssignmentService> _logger;

    private const int PICKUP_STOP_TIME_MINUTES = 3;
    private const int DELIVERY_STOP_TIME_MINUTES = 2;
    private const int ESTIMATED_ORDER_PREP_TIME_MINUTES = 12;

    private const int MAX_ASSIGNMENT_ATTEMPTS = 3;
    private const int DEFAULT_SEARCH_DISTANCE_METERS = 1500;

    private readonly TimeSpan _offerTimeout;
    private readonly TimeSpan _retryInterval;

    public DeliveryAssignmentService(
        IDriverRepository driverRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository,
        IJourneyCalculationService journeyCalculationService,
        IOrderRepository orderRepository,
        IHubContext<DriverHub> hubContext,
        IDeliveriesAssignments deliveriesAssignments,
        IDriverPaymentService driverPaymentService,
        ILogger<DeliveryAssignmentService> logger,
        TimeSpan? offerTimeout = null,
        TimeSpan? retryInterval = null
    )
    {
        _driverRepository = driverRepository;
        _foodPlaceRepository = foodPlaceRepository;
        _addressRepository = addressRepository;
        _orderRepository = orderRepository;
        _journeyCalculationService = journeyCalculationService;
        _hubContext = hubContext;
        _deliveriesAssignments = deliveriesAssignments;
        _driverPaymentService = driverPaymentService;
        _logger = logger;

        _offerTimeout = offerTimeout ?? TimeSpan.FromSeconds(30);
        _retryInterval = retryInterval ?? TimeSpan.FromSeconds(15);
    }

    public async Task<bool> InitiateDeliveryAssignment(Order order)
    {
        _logger.LogInformation("Initiating delivery assignment for order ID: {OrderId}", order.Id);
        var job = _deliveriesAssignments.CreateAssignmentJob(order.Id);
        var foodPlace = await _foodPlaceRepository.GetFoodPlaceById(order.FoodPlaceId);

        var didAssignDriver = false;
        while (!didAssignDriver)
        {
            if (job.CurrentAttempt < MAX_ASSIGNMENT_ATTEMPTS)
            {
                didAssignDriver = await TryAssignDriver(job, order, foodPlace!);
                if (!didAssignDriver)
                {
                    _logger.LogWarning(
                        "No driver accepted the delivery offer for order ID: {OrderId}. Scheduling retry attempt {Attempt}.",
                        job.OrderId,
                        job.CurrentAttempt + 1
                    );
                    await Task.Delay(_retryInterval);
                }
            }
            else
            {
                _logger.LogError(
                    "Max assignment attempts reached for order ID: {OrderId}. Could not assign a driver.",
                    job.OrderId
                );
                return false;
            }
        }
        return true;
    }

    private async Task<bool> TryAssignDriver(
        DeliveryAssignmentJob job,
        Order order,
        FoodPlace foodPlace
    )
    {
        job.CurrentAttempt++;
        var nearbyDrivers = await _driverRepository.GetAvailableDriversWithinDistance(
            foodPlace.Location.Latitude,
            foodPlace.Location.Longitude,
            DEFAULT_SEARCH_DISTANCE_METERS
        );

        if (!nearbyDrivers.Any())
        {
            _logger.LogWarning(
                "No available drivers found nearby for food place ID: {FoodPlaceId} for order ID: {OrderId}. Scheduling retry attempt {Attempt}.",
                foodPlace.Id,
                order.Id,
                job.CurrentAttempt + 1
            );
            return false;
        }

        foreach (var driver in nearbyDrivers.Shuffle())
        {
            var deliveryDto = await CreateDeliveryOfferDto(job, order, foodPlace, driver);
            if (await OfferDeliveryToDriver(job, deliveryDto, driver))
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> OfferDeliveryToDriver(
        DeliveryAssignmentJob job,
        DeliveryOfferDTO dto,
        AvailableDriver driver
    )
    {
        var cts = new CancellationTokenSource();
        job.Cts = cts;
        job.OfferedDriverId = driver.Id;

        driver.Status = DriverStatuses.offered;
        await _driverRepository.UpdateDriverStatus(driver);

        var connection = _hubContext.Clients.User(driver.Id.ToString());
        await connection.SendAsync("ReceiveDeliveryOffer", dto, job.OrderId);

        try
        {
            _logger.LogInformation(
                "Delivery offer sent to driver {DriverId} for order {OrderId}. Waiting for a response with timeout of {Timeout} seconds.",
                driver.Id,
                job.OrderId,
                _offerTimeout.Seconds
            );
            await Task.Delay(_offerTimeout, cts.Token);

            _logger.LogInformation(
                "Delivery offer to driver {DriverId} for order {OrderId} timed out. Cancelling offer.",
                driver.Id,
                job.OrderId
            );
            await CancelOfferForDriver(job, driver);
            return false;
        }
        catch (TaskCanceledException)
        {
            if (job.AssignedDriverId == driver.Id)
            {
                return true;
            }
            await CancelOfferForDriver(job, driver);
            return false;
        }
    }

    private async Task CancelOfferForDriver(DeliveryAssignmentJob job, AvailableDriver driver)
    {
        _logger.LogInformation(
            "Cancelling delivery offer for driver {DriverId} for order {OrderId}.",
            driver.Id,
            job.OrderId
        );
        job.Cts = null;
        job.OfferedDriverId = 0;

        var connection = _hubContext.Clients.User(driver.Id.ToString());
        await connection.SendAsync("DeliveryOfferCancelled");

        driver.Status = DriverStatuses.online;
        await _driverRepository.UpdateDriverStatus(driver);
    }

    public async Task AcceptDeliveryOffer(int driverId, int orderId)
    {
        var job =
            _deliveriesAssignments.GetAssignmentJob(orderId)
            ?? throw new Exception("Delivery assignment job was not found");

        if (job.OfferedDriverId != driverId)
        {
            _logger.LogWarning(
                "Unauthorised accept attempt: driver {DriverId} for order {OrderId}. Offer held by {OfferedDriverId}",
                driverId,
                orderId,
                job.OfferedDriverId
            );
            return;
        }

        job.AssignedDriverId = driverId;
        job.Cts!.Cancel();

        _logger.LogInformation(
            "Driver {DriverId} accepted delivery offer for order ID: {OrderId}",
            driverId,
            orderId
        );

        var driver = await _driverRepository.GetDriverById(driverId);
        if (driver == null)
        {
            _logger.LogError("Driver with ID {DriverId} not found.", driverId);
            throw new InvalidOperationException($"Driver with ID {driverId} not found.");
        }
        driver.Status = DriverStatuses.delivering;
        await _driverRepository.UpdateDriverStatus(driver);

        var order = await _orderRepository.GetOrderById(orderId);
        order!.Delivery!.DriverId = driverId;
        order.Delivery.Status = DeliveryStatuses.pickup;
        order.Delivery.RouteJson = job.Route;
        order.Delivery.PaymentAmount = job.Payment;

        await _orderRepository.UpdateDelivery(orderId, order.Delivery);

        var connection = _hubContext.Clients.User(driverId.ToString());
        await connection.SendAsync("AssignDelivery", orderId);

        _deliveriesAssignments.RemoveAssignmentJob(job.OrderId);
    }

    public void RejectDeliveryOffer(int driverId, int orderId)
    {
        var job =
            _deliveriesAssignments.GetAssignmentJob(orderId)
            ?? throw new Exception("Delivery assignment job was not found");
        ;
        if (job.OfferedDriverId != driverId)
        {
            _logger.LogWarning(
                "Unauthorised accept attempt: driver {DriverId} for order {OrderId}. Offer held by {OfferedDriverId}",
                driverId,
                orderId,
                job.OfferedDriverId
            );
            return;
        }
        job.Cts!.Cancel();
    }

    private async Task<(MapboxRouteInfo, string)> CalculateAndStoreRouteForDriver(
        DeliveryAssignmentJob job,
        AvailableDriver driver,
        Location foodPlaceLocation,
        Location customerLocation
    )
    {
        var driverLocation =
            driver.Location ?? throw new InvalidOperationException("Driver location not available");

        var (routeInfo, json) = await _journeyCalculationService.CalculateRouteAsync(
            new Location[] { driverLocation, foodPlaceLocation, customerLocation }
        );

        job.Route = json;

        return (routeInfo, json);
    }

    private int CalculateAndStorePaymentAmountForDriver(
        DeliveryAssignmentJob job,
        int driverId,
        double distance,
        double duration
    )
    {
        var amount = _driverPaymentService.CalculatePayment(distance, duration);

        job.Payment = amount;

        return amount;
    }

    private async Task<DeliveryOfferDTO> CreateDeliveryOfferDto(
        DeliveryAssignmentJob job,
        Order order,
        FoodPlace foodPlace,
        AvailableDriver availableDriver
    )
    {
        var customerLocation = await GetCustomerLocation(order);
        var (routeInfo, json) = await CalculateAndStoreRouteForDriver(
            job,
            availableDriver,
            foodPlace.Location,
            customerLocation
        );

        var timeToFoodPlace = routeInfo.Legs[0].Duration;
        var pickupStopTime = TimeSpan.FromMinutes(PICKUP_STOP_TIME_MINUTES);
        var pickUpTime = (int)(TimeSpan.FromSeconds(timeToFoodPlace) + pickupStopTime).TotalSeconds;

        var orderPrepTime = (int)
            TimeSpan.FromMinutes(ESTIMATED_ORDER_PREP_TIME_MINUTES).TotalSeconds;

        var deliveryStopTime = TimeSpan.FromMinutes(DELIVERY_STOP_TIME_MINUTES);
        var totalTime = (int)
            (
                TimeSpan.FromSeconds(routeInfo.Duration) + pickupStopTime + deliveryStopTime
            ).TotalSeconds;

        var paymentAmount = CalculateAndStorePaymentAmountForDriver(
            job,
            availableDriver.Id,
            routeInfo.Distance,
            routeInfo.Duration
        );

        var foodPlaceAddress =
            await _addressRepository.GetAddressById(foodPlace.AddressId)
            ?? throw new InvalidOperationException("Foodplace address not found");

        return new DeliveryOfferDTO
        {
            FoodPlaceName = foodPlace.Name,
            FoodPlaceAddress = foodPlaceAddress,
            EstimatedOrderPreparationTime = orderPrepTime,
            EstimatedPickupTime = pickUpTime,
            TotalDistance = routeInfo.Distance,
            TotalEstimatedTime = totalTime,
            PaymentAmount = paymentAmount,
            RouteDataJson =
                JsonSerializer.Deserialize<object>(json)
                ?? throw new Exception("Could not serialise the route string to json"),
        };
    }

    private async Task<Location> GetCustomerLocation(Order order)
    {
        var deliveryDestinationAddress =
            await _addressRepository.GetAddressById(order.DeliveryAddressId)
            ?? throw new InvalidOperationException("Delivery destination address not found");

        var addressString =
            $"{deliveryDestinationAddress.NumberAndStreet}, {deliveryDestinationAddress.City}, {deliveryDestinationAddress.Postcode}";
        var customerLocation = await _journeyCalculationService.GeocodeAddressAsync(addressString);

        if (customerLocation == null)
        {
            throw new InvalidOperationException(
                $"Could not geocode customer address: {addressString}"
            );
        }

        return customerLocation;
    }
}
