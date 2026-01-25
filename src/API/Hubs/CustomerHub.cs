using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize(Roles = nameof(UserTypes.customer))]
public class CustomerHub : Hub
{
    private readonly ICustomerConnections _customerConnections;
    private readonly ILogger<CustomerHub> _logger;

    public CustomerHub(
        ICustomerConnections customerConnections,
        ILogger<CustomerHub> logger
    )
    {
        _customerConnections = customerConnections;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        string customerId = GetCustomerId();
        _customerConnections.AddConnection(int.Parse(customerId));

        await Clients.Caller.SendAsync("Connected", "Successfully connected to delivery tracking");
        _logger.LogInformation("Customer with ID: {CustomerId} connected", customerId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        string customerId = GetCustomerId();
        _customerConnections.RemoveConnection(int.Parse(customerId));

        _logger.LogInformation("Customer with ID: {CustomerId} disconnected", customerId);
        await base.OnDisconnectedAsync(e);
    }

    private string GetCustomerId()
    {
        return Context.UserIdentifier
            ?? throw new Exception("Could not get the customerId");
    }
}
