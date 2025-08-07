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

    public DeliveryAssignmentService(
        IDriverRepository driverRepository,
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository,
        JourneyCalculationService journeyCalculationService,
        IOrderRepository orderRepository,
        IHubContext<DriverHub> hubContext,
        DeliveriesAssignments deliveriesAssignments
    )
    {
        _driverRepository = driverRepository;
        _foodPlaceRepo = foodPlaceRepository;
        _addressRepository = addressRepository;
        _orderRepository = orderRepository;
        _journeryCalcService = journeyCalculationService;
        _hubContext = hubContext;
        _deliveriesAssignments = deliveriesAssignments;
    }

    public async Task InitiateDeliveryAssignment(Order order)
    {
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
            Console.WriteLine(
                $"No drivers available nearby food place id {foodPlace.Id} for order {order.Id}. Scheduling retry."
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
            Console.WriteLine(
                $"No driver accepted the offer for order {job.OrderId}. Scheduling retry."
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
            Console.WriteLine(
                $"Max assignment attempts reached for order {job.OrderId}. Could not assign a driver."
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
                await Task.Delay(_offerTimeout, cts.Token);

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
