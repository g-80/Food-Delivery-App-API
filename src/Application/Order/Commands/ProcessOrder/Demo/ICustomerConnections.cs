public interface ICustomerConnections
{
    void AddConnection(int customerId);
    void RemoveConnection(int customerId);
    bool IsConnected(int customerId);
    Task<bool> WaitForCustomerConnection(
        int customerId,
        TimeSpan timeout,
        CancellationToken ct = default
    );
}
