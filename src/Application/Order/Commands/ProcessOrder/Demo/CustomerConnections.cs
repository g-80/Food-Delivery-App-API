using System.Collections.Concurrent;

public class CustomerConnections : ICustomerConnections
{
    private readonly ConcurrentDictionary<int, CustomerConnectionInfo> _connections = new();

    public void AddConnection(int customerId)
    {
        var info = _connections.GetOrAdd(customerId, _ => new CustomerConnectionInfo());
        info.IsConnected = true;
        info.ConnectedTcs?.TrySetResult(true);
    }

    public void RemoveConnection(int customerId)
    {
        if (_connections.TryGetValue(customerId, out var info))
        {
            info.IsConnected = false;
        }
    }

    public bool IsConnected(int customerId)
    {
        return _connections.TryGetValue(customerId, out var info) && info.IsConnected;
    }

    public async Task<bool> WaitForCustomerConnection(
        int customerId,
        TimeSpan timeout,
        CancellationToken ct = default
    )
    {
        var info = _connections.GetOrAdd(customerId, _ => new CustomerConnectionInfo());

        if (info.IsConnected)
            return true;

        info.ConnectedTcs = new TaskCompletionSource<bool>();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            var completedTask = await Task.WhenAny(
                info.ConnectedTcs.Task,
                Task.Delay(Timeout.Infinite, cts.Token)
            );

            return completedTask == info.ConnectedTcs.Task && info.ConnectedTcs.Task.Result;
        }
        catch (OperationCanceledException)
        {
            return info.IsConnected;
        }
    }

    private class CustomerConnectionInfo
    {
        public bool IsConnected { get; set; }
        public TaskCompletionSource<bool>? ConnectedTcs { get; set; }
    }
}
