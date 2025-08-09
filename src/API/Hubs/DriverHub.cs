using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize(Roles = nameof(UserTypes.driver))]
public class DriverHub : Hub
{
    private readonly GoOnlineHandler _goOnlineHandler;
    private readonly DisconnectDriverHandler _disconnectDriverHandler;
    private readonly UpsertLocationHandler _upsertLocationHandler;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;
    private readonly ILogger<DriverHub> _logger;

    public DriverHub(
        GoOnlineHandler goOnlineHandler,
        DisconnectDriverHandler disconnectDriverHandler,
        UpsertLocationHandler upsertLocationHandler,
        IDeliveryAssignmentService deliveryAssignmentService,
        ILogger<DriverHub> logger
    )
    {
        _goOnlineHandler = goOnlineHandler;
        _disconnectDriverHandler = disconnectDriverHandler;
        _upsertLocationHandler = upsertLocationHandler;
        _deliveryAssignmentService = deliveryAssignmentService;
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

    public async Task UpdateLocation(double latitude, double longitude)
    {
        string driverId = GetDriverId();
        await _upsertLocationHandler.Handle(
            int.Parse(driverId),
            new Location { Latitude = latitude, Longitude = longitude }
        );
        _logger.LogInformation(
            "Driver with ID: {DriverId} updated location to Latitude: {Latitude}, Longitude: {Longitude}",
            driverId,
            latitude,
            longitude
        );
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

    private string GetDriverId()
    {
        string driverId =
            Context.UserIdentifier ?? throw new Exception("Could not get the driverId");
        return driverId;
    }
}
