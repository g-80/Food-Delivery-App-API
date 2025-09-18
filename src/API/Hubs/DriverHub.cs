using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize(Roles = nameof(UserTypes.driver))]
public class DriverHub : Hub
{
    private readonly GoOnlineHandler _goOnlineHandler;
    private readonly DisconnectDriverHandler _disconnectDriverHandler;
    private readonly UpsertLocationHandler _upsertLocationHandler;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;
    private readonly AddDriverLocationHistoryHandler _addLocationHistoryHandler;
    private readonly AddDriverLocationHistoryValidator _locationValidator;
    private readonly UpdateETAHandler _updateETAHandler;
    private readonly ILogger<DriverHub> _logger;

    public DriverHub(
        GoOnlineHandler goOnlineHandler,
        DisconnectDriverHandler disconnectDriverHandler,
        UpsertLocationHandler upsertLocationHandler,
        IDeliveryAssignmentService deliveryAssignmentService,
        AddDriverLocationHistoryHandler addLocationHistoryHandler,
        AddDriverLocationHistoryValidator locationValidator,
        UpdateETAHandler updateETAHandler,
        ILogger<DriverHub> logger
    )
    {
        _goOnlineHandler = goOnlineHandler;
        _disconnectDriverHandler = disconnectDriverHandler;
        _upsertLocationHandler = upsertLocationHandler;
        _deliveryAssignmentService = deliveryAssignmentService;
        _addLocationHistoryHandler = addLocationHistoryHandler;
        _locationValidator = locationValidator;
        _updateETAHandler = updateETAHandler;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        string driverId = GetDriverId();
        await _goOnlineHandler.Handle(int.Parse(driverId));
        await Clients.Caller.SendAsync("StatusUpdated", "Online");
        _logger.LogInformation("Driver with ID: {DriverId} connected", driverId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        string driverId = GetDriverId();
        await _disconnectDriverHandler.Handle(int.Parse(driverId));
        _logger.LogInformation("Driver with ID: {DriverId} disconnected", driverId);
        await base.OnDisconnectedAsync(e);
    }

    public async Task UpdateLocation(
        double latitude,
        double longitude,
        double? accuracy = null,
        double? speed = null,
        double? heading = null,
        int? deliveryId = null
    )
    {
        string driverId = GetDriverId();
        var location = new Location { Latitude = latitude, Longitude = longitude };

        await _upsertLocationHandler.Handle(int.Parse(driverId), location);

        if (deliveryId != null)
        {
            var command = new AddDriverLocationHistoryCommand
            {
                DriverId = int.Parse(driverId),
                Location = location,
                Accuracy = accuracy ?? 0,
                Speed = speed ?? 0,
                Heading = heading ?? 0,
                DeliveryId = deliveryId.Value,
            };

            var validationResult = _locationValidator.Validate(command);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors);
                await Clients.Caller.SendAsync("LocationUpdateError", errorMessage);

                _logger.LogWarning(
                    "Location validation failed for driver {DriverId}: {Errors}",
                    driverId,
                    errorMessage
                );
                return;
            }

            await _addLocationHistoryHandler.Handle(command);

            _logger.LogDebug(
                "Driver {DriverId} updated delivery tracking location to Lat: {Latitude}, Lng: {Longitude} with accuracy: {Accuracy}m, speed: {Speed}m/s, heading: {Heading}° for delivery: {DeliveryId}",
                driverId,
                latitude,
                longitude,
                accuracy,
                speed,
                heading,
                deliveryId
            );
        }
        else
        {
            _logger.LogDebug(
                "Driver with ID: {DriverId} updated location to Latitude: {Latitude}, Longitude: {Longitude}",
                driverId,
                latitude,
                longitude
            );
        }
    }

    public async Task AcceptDeliveryOffer(int orderId)
    {
        string driverId = GetDriverId();
        _logger.LogInformation(
            "Received delivery offer acceptance from driver with ID: {DriverId} for Order ID: {OrderId}",
            driverId,
            orderId
        );
        await _deliveryAssignmentService.AcceptDeliveryOffer(int.Parse(driverId), orderId);
    }

    public void RejectDeliveryOffer(int orderId)
    {
        string driverId = GetDriverId();
        _logger.LogInformation(
            "Received delivery offer rejection from driver with ID: {DriverId} for Order ID: {OrderId}",
            driverId,
            orderId
        );
        _deliveryAssignmentService.RejectDeliveryOffer(int.Parse(driverId), orderId);
    }

    public async Task UpdateETA(int deliveryId, TimeSpan newETA)
    {
        string driverId = GetDriverId();

        try
        {
            var command = new UpdateETACommand
            {
                DriverId = int.Parse(driverId),
                DeliveryId = deliveryId,
                NewETA = newETA,
            };

            await _updateETAHandler.Handle(command);

            _logger.LogInformation(
                "Driver {DriverId} updated ETA for delivery {DeliveryId} to {NewETA}",
                driverId,
                deliveryId,
                newETA
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating ETA for driver {DriverId} and delivery {DeliveryId}",
                driverId,
                deliveryId
            );
            throw;
        }
    }

    private string GetDriverId()
    {
        string driverId =
            Context.UserIdentifier ?? throw new Exception("Could not get the driverId");
        return driverId;
    }
}
