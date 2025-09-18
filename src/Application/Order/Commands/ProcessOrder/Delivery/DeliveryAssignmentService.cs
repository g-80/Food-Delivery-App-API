using Microsoft.AspNetCore.SignalR;

public class DeliveryAssignmentService : IDeliveryAssignmentService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepo;
    private readonly IAddressRepository _addressRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IJourneyCalculationService _journeryCalcService;
    private readonly IHubContext<DriverHub> _hubContext;
    private readonly IDeliveriesAssignments _deliveriesAssignments;
    private readonly ILogger<DeliveryAssignmentService> _logger;

    private readonly TimeSpan _offerTimeout;
    private readonly TimeSpan _retryInterval;
    private readonly int _maxAssignmentAttempts = 3;
    private readonly int _defaultDistance = 1500;

    public DeliveryAssignmentService(
        IDriverRepository driverRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository,
        IJourneyCalculationService journeyCalculationService,
        IOrderRepository orderRepository,
        IHubContext<DriverHub> hubContext,
        IDeliveriesAssignments deliveriesAssignments,
        ILogger<DeliveryAssignmentService> logger,
        TimeSpan? offerTimeout = null,
        TimeSpan? retryInterval = null
    )
    {
        _driverRepository = driverRepository;
        _foodPlaceRepo = foodPlaceRepository;
        _addressRepository = addressRepository;
        _orderRepository = orderRepository;
        _journeryCalcService = journeyCalculationService;
        _hubContext = hubContext;
        _deliveriesAssignments = deliveriesAssignments;
        _logger = logger;

        _offerTimeout = offerTimeout ?? TimeSpan.FromSeconds(30);
        _retryInterval = retryInterval ?? TimeSpan.FromSeconds(15);
    }

    public async Task<bool> InitiateDeliveryAssignment(Order order)
    {
        _logger.LogInformation("Initiating delivery assignment for order ID: {OrderId}", order.Id);
        var job = _deliveriesAssignments.CreateAssignmentJob(order.Id);
        var foodPlace = await _foodPlaceRepo.GetFoodPlaceById(order.FoodPlaceId);

        var foodPlaceToCustomerRoute = await CalculateRouteFoodPlaceToCustomer(order, foodPlace!);

        var didAssignDriver = false;
        while (!didAssignDriver)
        {
            if (job.CurrentAttempt < _maxAssignmentAttempts)
            {
                didAssignDriver = await TryAssignDriver(
                    job,
                    order,
                    foodPlace!,
                    foodPlaceToCustomerRoute
                );
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
                _deliveriesAssignments.RemoveAssignmentJob(job.OrderId);
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
        FoodPlace foodPlace,
        MapboxRoute foodPlaceToCustomerRoute
    )
    {
        job.CurrentAttempt++;
        var nearbyDrivers = await _driverRepository.GetAvailableDriversWithinDistance(
            foodPlace.Location.Latitude,
            foodPlace.Location.Longitude,
            _defaultDistance
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

        var tasks = nearbyDrivers
            .Select(async driver =>
            {
                var deliveryDto = await CreateDeliveryOfferDto(
                    job,
                    order.DeliveryAddressId,
                    foodPlace,
                    driver,
                    foodPlaceToCustomerRoute
                );
                return await OfferDeliveryToDriver(job, deliveryDto, driver);
            })
            .ToList();

        var res = await Task.WhenAll(tasks);
        return res.Any(r => r == true);
    }

    private async Task<bool> OfferDeliveryToDriver(
        DeliveryAssignmentJob job,
        DeliveryOfferDTO dto,
        AvailableDriver driver
    )
    {
        var cts = new CancellationTokenSource();
        job.PendingOffers[driver.Id] = cts;

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
            // if-check in case of a race condition
            if (!cts.IsCancellationRequested)
            {
                await CancelOfferForDriver(job, driver);
                return false;
            }
            // race condition - driver either accepted or rejected the offer
            return job.AssignedDriverId == driver.Id;
        }
        catch (TaskCanceledException)
        {
            await CancelOfferForDriver(job, driver);
            return job.AssignedDriverId == driver.Id;
        }
    }

    private async Task CancelOfferForDriver(DeliveryAssignmentJob job, AvailableDriver driver)
    {
        if (job.AssignedDriverId == driver.Id)
        {
            return;
        }

        _logger.LogInformation(
            "Cancelling delivery offer for driver {DriverId} for order {OrderId}.",
            driver.Id,
            job.OrderId
        );
        job.PendingOffers.TryRemove(driver.Id, out _);

        var connection = _hubContext.Clients.User(driver.Id.ToString());
        await connection.SendAsync("DeliveryOfferCancelled");

        driver.Status = DriverStatuses.online;
        await _driverRepository.UpdateDriverStatus(driver);
    }

    public async Task AcceptDeliveryOffer(int driverId, int orderId)
    {
        var job = _deliveriesAssignments.GetAssignmentJob(orderId);

        // Check if a driver is already assigned to this delivery in case of a delay
        if (job.AssignedDriverId != 0)
        {
            return;
        }

        job.AssignedDriverId = driverId;

        _logger.LogInformation(
            "Driver {DriverId} accepted delivery offer for order ID: {OrderId}. Cancelling pending offers for other drivers.",
            driverId,
            orderId
        );
        CancelAllPendingOffers(job);

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

        if (job.DriversRoutes.TryGetValue(driverId, out var route))
        {
            order.Delivery.Route = route;
        }

        await _orderRepository.UpdateDelivery(orderId, order.Delivery);

        var connection = _hubContext.Clients.User(driverId.ToString());
        await connection.SendAsync("AssignDelivery", orderId);

        _deliveriesAssignments.RemoveAssignmentJob(job.OrderId);
    }

    public void RejectDeliveryOffer(int driverId, int orderId)
    {
        var job = _deliveriesAssignments.GetAssignmentJob(orderId);

        if (job.PendingOffers.TryRemove(driverId, out var cts))
        {
            cts.Cancel();
        }
    }

    public void CancelOngoingAssignment(int orderId)
    {
        var job = _deliveriesAssignments.GetAssignmentJob(orderId);
        if (job == null)
            return;

        _logger.LogInformation("Cancelling delivery assignment for order ID: {OrderId}", orderId);

        CancelAllPendingOffers(job);

        _deliveriesAssignments.RemoveAssignmentJob(orderId);
    }

    private void CancelAllPendingOffers(DeliveryAssignmentJob job)
    {
        var pendingDriversIds = job.PendingOffers.Keys.ToList();
        foreach (var driverId in pendingDriversIds)
        {
            if (job.PendingOffers.TryRemove(driverId, out var cts))
            {
                cts.Cancel();
            }
        }
    }

    private async Task<DeliveryOfferDTO> CreateDeliveryOfferDto(
        DeliveryAssignmentJob job,
        int deliveryAddressId,
        FoodPlace foodPlace,
        AvailableDriver availableDriver,
        MapboxRoute foodPlaceToCustomerRoute
    )
    {
        var foodPlaceAddress =
            await _addressRepository.GetAddressById(foodPlace.AddressId)
            ?? throw new InvalidOperationException("Foodplace address not found");
        var deliveryDestinationAddress =
            await _addressRepository.GetAddressById(deliveryAddressId)
            ?? throw new InvalidOperationException("Delivery destination address not found");

        var driverLocation =
            availableDriver.Location
            ?? throw new InvalidOperationException("Driver location not available");

        var driverToFoodPlaceRoute = await _journeryCalcService.CalculateRouteAsync(
            driverLocation,
            foodPlace.Location
        );

        var pickupStopTime = TimeSpan.FromMinutes(3);
        var deliveryStopTime = TimeSpan.FromMinutes(2);

        var combinedRoute = _journeryCalcService.CreateCombinedRoute(
            driverToFoodPlaceRoute,
            foodPlaceToCustomerRoute
        );

        job.DriversRoutes[availableDriver.Id] = combinedRoute;

        return new DeliveryOfferDTO
        {
            FoodPlaceName = foodPlace.Name,
            FoodPlaceAddress = foodPlaceAddress,
            DistanceToFoodPlace = driverToFoodPlaceRoute.Distance,
            EstimatedOrderPreparationTime = TimeSpan.FromMinutes(12),
            EstimatedPickupTime =
                TimeSpan.FromSeconds(driverToFoodPlaceRoute.Duration) + pickupStopTime,

            DeliveryDestinationAddress = deliveryDestinationAddress,
            DistanceToDeliveryDestination = foodPlaceToCustomerRoute.Distance,
            EstimatedDeliveryTime =
                TimeSpan.FromSeconds(foodPlaceToCustomerRoute.Duration) + deliveryStopTime,

            TotalDistance = combinedRoute.Distance,
            TotalEstimatedTime =
                TimeSpan.FromSeconds(combinedRoute.Duration) + pickupStopTime + deliveryStopTime,
            Route = combinedRoute,
        };
    }

    private async Task<MapboxRoute> CalculateRouteFoodPlaceToCustomer(
        Order order,
        FoodPlace foodPlace
    )
    {
        var deliveryDestinationAddress =
            await _addressRepository.GetAddressById(order.DeliveryAddressId)
            ?? throw new InvalidOperationException("Delivery destination address not found");

        var addressString =
            $"{deliveryDestinationAddress.NumberAndStreet}, {deliveryDestinationAddress.City}, {deliveryDestinationAddress.Postcode}";
        var customerLocation = await _journeryCalcService.GeocodeAddressAsync(addressString);

        if (customerLocation == null)
        {
            throw new InvalidOperationException(
                $"Could not geocode customer address: {addressString}"
            );
        }

        return await _journeryCalcService.CalculateRouteAsync(foodPlace.Location, customerLocation);
    }
}
