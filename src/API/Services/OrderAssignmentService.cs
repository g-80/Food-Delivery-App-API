using Microsoft.AspNetCore.SignalR;

public class OrderAssignmentService
{
    private readonly DriversService _driversService;
    private readonly IOrdersRepository _ordersRepo;
    private readonly IFoodPlacesRepository _foodPlacesRepo;
    private readonly AddressesService _addressesService;
    private readonly DeliveriesService _deliveriesService;
    private readonly JourneyCalculationService _journeryCalcService;
    private readonly IHubContext<DriverHub> _hubContext;
    private readonly OrdersAssignments _ordersAssignments;
    private readonly TimeSpan _offerTimeout = TimeSpan.FromSeconds(30);
    private readonly int _maxAssignmentAttempts = 3;
    private int _defaultDistance = 1500;

    public OrderAssignmentService(
        DriversService driversService,
        IOrdersRepository ordersRepository,
        IFoodPlacesRepository foodPlacesRepository,
        AddressesService addressesService,
        JourneyCalculationService journeyCalculationService,
        DeliveriesService deliveriesService,
        IHubContext<DriverHub> hubContext,
        OrdersAssignments ordersAssignments
    )
    {
        _driversService = driversService;
        _ordersRepo = ordersRepository;
        _foodPlacesRepo = foodPlacesRepository;
        _addressesService = addressesService;
        _deliveriesService = deliveriesService;
        _journeryCalcService = journeyCalculationService;
        _hubContext = hubContext;
        _ordersAssignments = ordersAssignments;
    }

    public async Task AssignOrderToDriver(int orderId)
    {
        var job = _ordersAssignments.GetOrCreateAssignmentJob(orderId);
        job.CurrentAttempt++;

        var order = await _ordersRepo.GetOrderById(orderId);
        var foodPlace = await _foodPlacesRepo.GetFoodPlace(order!.FoodPlaceId);
        var nearbyDrivers = await _driversService.GetAvailableDriversWithinDistance(
            foodPlace!.Latitude,
            foodPlace.Longitude,
            _defaultDistance
        );

        if (!nearbyDrivers.Any())
        {
            Console.WriteLine($"No drivers available nearby order {orderId}. Scheduling retry.");
            await ScheduleRetryAttempt(job);
            return;
        }

        foreach (var driver in nearbyDrivers)
        {
            var orderDto = await CreateOrderOfferDto(order!, foodPlace, driver);
            await OfferOrderToDriver(job, orderDto, driver.DriverId);
        }

        _ = CheckAssignmentSuccess(job);
    }

    private async Task CheckAssignmentSuccess(OrderAssignmentJob job)
    {
        await Task.Delay(_offerTimeout);

        if (job.AssignedDriverId == 0)
        {
            Console.WriteLine(
                $"No driver accepted the offer for order {job.OrderId}. Scheduling retry."
            );
            await ScheduleRetryAttempt(job);
        }
    }

    private async Task ScheduleRetryAttempt(OrderAssignmentJob job)
    {
        if (job.CurrentAttempt < _maxAssignmentAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(15));
            await AssignOrderToDriver(job.OrderId);
        }
        else
        {
            _ordersAssignments.RemoveAssignmentJob(job.OrderId);
            Console.WriteLine(
                $"Max assignment attempts reached for order {job.OrderId}. Could not assign a driver."
            );
            // Handle the case where no driver could be assigned
        }
    }

    private async Task OfferOrderToDriver(OrderAssignmentJob job, OrderOfferDTO dto, int driverId)
    {
        var cts = new CancellationTokenSource();
        job.PendingOffers[driverId] = cts;

        await _driversService.UpdateDriverStatus(driverId, DriverStatuses.offered);

        var connection = _hubContext.Clients.User(driverId.ToString());
        await connection.SendAsync("OfferOrder", dto, job.OrderId);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_offerTimeout, cts.Token);

                // if-check in case of a race condition
                if (!cts.IsCancellationRequested)
                {
                    await CancelOfferForDriver(job, driverId);
                }
            }
            catch (TaskCanceledException)
            {
                await CancelOfferForDriver(job, driverId);
            }
        });
    }

    private async Task CancelOfferForDriver(OrderAssignmentJob job, int driverId)
    {
        if (job.AssignedDriverId == driverId)
        {
            return;
        }

        job.PendingOffers.TryRemove(driverId, out _);

        var connection = _hubContext.Clients.User(driverId.ToString());
        await connection.SendAsync("OrderOfferCancelled");

        await _driversService.UpdateDriverStatus(driverId, DriverStatuses.online);
    }

    public async Task AcceptOrderOffer(int driverId, int orderId)
    {
        var job = _ordersAssignments.GetAssignmentJob(orderId);

        // Check if a driver is already assigned to this order in case of a delay
        if (job.AssignedDriverId != 0)
        {
            return;
        }

        job.AssignedDriverId = driverId;

        CancelAllPendingOffers(job);

        await _driversService.UpdateDriverStatus(driverId, DriverStatuses.delivering);

        await _deliveriesService.CreateDeliveryAsync(orderId, driverId);

        var connection = _hubContext.Clients.User(driverId.ToString());
        await connection.SendAsync("OrderAssigned", orderId);

        _ordersAssignments.RemoveAssignmentJob(job.OrderId);
    }

    public void RejectOrderOffer(int driverId, int orderId)
    {
        var job = _ordersAssignments.GetAssignmentJob(orderId);

        if (job.PendingOffers.TryRemove(driverId, out var cts))
        {
            cts.Cancel();
        }
    }

    private void CancelAllPendingOffers(OrderAssignmentJob job)
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

    private async Task<OrderOfferDTO> CreateOrderOfferDto(
        Order order,
        FoodPlace foodPlace,
        AvailableDriverDTO availableDriver
    )
    {
        var foodPlaceAddress =
            await _addressesService.GetAddressById(foodPlace.AddressId)
            ?? throw new Exception("Foodplace address not found");
        var deliveryDestinationAddress =
            await _addressesService.GetAddressById(order.DeliveryAddressId)
            ?? throw new Exception("Delivery destination address not found");

        return new OrderOfferDTO
        {
            FoodPlace = foodPlace.Name,
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
