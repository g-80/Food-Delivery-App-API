using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize(Roles = nameof(UserTypes.driver))]
public class DriverHub : Hub
{
    private readonly DriversService _driversService;
    private readonly OrderAssignmentService _orderAssignmentService;

    public DriverHub(DriversService driversService, OrderAssignmentService orderAssignmentService)
    {
        _driversService = driversService;
        _orderAssignmentService = orderAssignmentService;
    }

    public override async Task OnConnectedAsync()
    {
        string driverId = GetDriverId();
        await _driversService.CreateDriverStatusAsync(int.Parse(driverId));
        await Clients.Caller.SendAsync("StatusUpdated", "Online");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        string driverId = GetDriverId();
        await _driversService.RemoveDriverStatusAsync(int.Parse(driverId));
        await _driversService.RemoveDriverLocationAsync(int.Parse(driverId));
        await base.OnDisconnectedAsync(e);
    }

    public async Task UpdateLocation(double latitude, double longitude)
    {
        string driverId = GetDriverId();
        await _driversService.UpsertDriverLocationAsync(int.Parse(driverId), latitude, longitude);
    }

    public async Task AcceptOrderOffer(int orderId)
    {
        string driverId = GetDriverId();
        await _orderAssignmentService!.AcceptOrderOffer(int.Parse(driverId), orderId);
    }

    public void RejectOrderOffer(int orderId)
    {
        string driverId = GetDriverId();
        _orderAssignmentService!.RejectOrderOffer(int.Parse(driverId), orderId);
    }

    private string GetDriverId()
    {
        string driverId =
            Context.UserIdentifier ?? throw new Exception("Could not get the driverId");
        return driverId;
    }
}
