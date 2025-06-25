using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize(Roles = nameof(UserTypes.food_place))]
public class FoodPlaceHub : Hub
{
    public FoodPlaceHub() { }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("StatusUpdated", "Online");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        await base.OnDisconnectedAsync(e);
    }
}
