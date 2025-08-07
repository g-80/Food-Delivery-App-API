using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize(Roles = nameof(UserTypes.driver))]
public class DriverHub : Hub
{
    private readonly GoOnlineHandler _goOnlineHandler;
    private readonly DisconnectDriverHandler _disconnectDriverHandler;
    private readonly UpsertLocationHandler _upsertLocationHandler;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;

    public DriverHub(
        GoOnlineHandler goOnlineHandler,
        DisconnectDriverHandler disconnectDriverHandler,
        UpsertLocationHandler upsertLocationHandler,
        IDeliveryAssignmentService deliveryAssignmentService
    )
    {
        _goOnlineHandler = goOnlineHandler;
        _disconnectDriverHandler = disconnectDriverHandler;
        _upsertLocationHandler = upsertLocationHandler;
        _deliveryAssignmentService = deliveryAssignmentService;
    }

    public override async Task OnConnectedAsync()
    {
        string driverId = GetDriverId();
        await _goOnlineHandler.Handle(int.Parse(driverId));
        await Clients.Caller.SendAsync("StatusUpdated", "Online");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        string driverId = GetDriverId();
        await _disconnectDriverHandler.Handle(int.Parse(driverId));
        await base.OnDisconnectedAsync(e);
    }

    public async Task UpdateLocation(double latitude, double longitude)
    {
        string driverId = GetDriverId();
        await _upsertLocationHandler.Handle(
            int.Parse(driverId),
            new Location { Latitude = latitude, Longitude = longitude }
        );
    }

    public async Task AcceptOrderOffer(int orderId)
    {
        string driverId = GetDriverId();
        await _deliveryAssignmentService.AcceptDeliveryOffer(int.Parse(driverId), orderId);
    }

    public void RejectOrderOffer(int orderId)
    {
        string driverId = GetDriverId();
        _deliveryAssignmentService.RejectDeliveryOffer(int.Parse(driverId), orderId);
    }

    private string GetDriverId()
    {
        string driverId =
            Context.UserIdentifier ?? throw new Exception("Could not get the driverId");
        return driverId;
    }
}
