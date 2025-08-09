using Microsoft.AspNetCore.SignalR;

public class DeliveryAssignmentService : IDeliveryAssignmentService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IFoodPlaceRepository _foodPlaceRepo;
    private readonly IAddressRepository _addressRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly JourneyCalculationService _journeryCalcService;
    private readonly IHubContext<DriverHub> _hubContext;
    private readonly DeliveriesAssignments _deliveriesAssignments;

    private readonly TimeSpan _offerTimeout = TimeSpan.FromSeconds(30);
    private readonly int _maxAssignmentAttempts = 3;
    private readonly int _defaultDistance = 1500;
    private readonly ILogger<DeliveryAssignmentService> _logger;

    public DeliveryAssignmentService(
        IDriverRepository driverRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository,
        JourneyCalculationService journeyCalculationService,
        IOrderRepository orderRepository,
        IHubContext<DriverHub> hubContext,
        DeliveriesAssignments deliveriesAssignments,
        ILogger<DeliveryAssignmentService> logger
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
    }

    public async Task InitiateDeliveryAssignment(Order order)
    {
        _logger.LogInformation("Initiating delivery assignment for order ID: {OrderId}", order.Id);
        var job = _deliveriesAssignments.GetOrCreateAssignmentJob(order.Id);
        job.CurrentAttempt++;

        var foodPlace = await _foodPlaceRepo.GetFoodPlaceById(order.FoodPlaceId);
        var nearbyDrivers = await _driverRepository.GetAvailableDriversWithinDistance(
            foodPlace!.Location.Latitude,
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
            await ScheduleRetryAttempt(job, order);
            return;
        }

        foreach (var driver in nearbyDrivers)
        {
            var deliveryDto = await CreateDeliveryOfferDto(
                order.DeliveryAddressId,
                foodPlace,
                driver
            );
            await OfferDeliveryToDriver(job, deliveryDto, driver);
        }

        _ = CheckAssignmentSuccess(job, order);
    }

    private async Task CheckAssignmentSuccess(DeliveryAssignmentJob job, Order order)
    {
        await Task.Delay(_offerTimeout);

        if (job.AssignedDriverId == 0)
        {
            _logger.LogWarning(
                "No driver accepted the delivery offer for order ID: {OrderId}. Scheduling retry attempt {Attempt}.",
                job.OrderId,
                job.CurrentAttempt + 1
            );
            await ScheduleRetryAttempt(job, order);
        }
    }

    private async Task ScheduleRetryAttempt(DeliveryAssignmentJob job, Order order)
    {
        if (job.CurrentAttempt < _maxAssignmentAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(15));
            await InitiateDeliveryAssignment(order);
        }
        else
        {
            _deliveriesAssignments.RemoveAssignmentJob(job.OrderId);
            _logger.LogError(
                "Max assignment attempts reached for order ID: {OrderId}. Could not assign a driver.",
                job.OrderId
            );
            // Handle the case where no driver could be assigned
        }
    }

    private async Task OfferDeliveryToDriver(
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

        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation(
                    "Delivery offer sent to driver {DriverId} for order {OrderId}. Waiting for a response with timeout of {Timeout} seconds.",
                    driver.Id,
                    job.OrderId,
                    _offerTimeout
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
                }
            }
            catch (TaskCanceledException)
            {
                await CancelOfferForDriver(job, driver);
            }
        });
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
        driver!.Status = DriverStatuses.delivering;
        await _driverRepository.UpdateDriverStatus(driver);

        var order = await _orderRepository.GetOrderById(orderId);
        order!.Delivery!.DriverId = driverId;
        order.Delivery.Status = DeliveryStatuses.pickup;
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
        int deliveryAddressId,
        FoodPlace foodPlace,
        AvailableDriver availableDriver
    )
    {
        var foodPlaceAddress =
            await _addressRepository.GetAddressById(foodPlace.AddressId)
            ?? throw new Exception("Foodplace address not found");
        var deliveryDestinationAddress =
            await _addressRepository.GetAddressById(deliveryAddressId)
            ?? throw new Exception("Delivery destination address not found");

        return new DeliveryOfferDTO
        {
            FoodPlaceName = foodPlace.Name,
            FoodPlaceAddress = foodPlaceAddress,
            DistanceToFoodPlace = (int)availableDriver.Distance,
            EstimatedTimeToFoodPlace = _journeryCalcService.CalculateEstimatedTimeToDestination(),
            EstimatedOrderPreparationTime = TimeSpan.FromMinutes(12),
            DeliveryDestinationAddress = deliveryDestinationAddress,
            DistanceToDeliveryDestination = _journeryCalcService.CalculateDistanceToDestination(),
            EstimatedTimeToDeliveryDestination =
                _journeryCalcService.CalculateEstimatedTimeToDestination(),
        };
    }
}
