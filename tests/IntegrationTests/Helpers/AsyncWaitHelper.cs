namespace IntegrationTests.Helpers;

public static class AsyncWaitHelper
{
    /// <summary>
    /// Polls the given condition until it returns true or the timeout expires.
    /// Use this instead of Task.Delay when waiting for async state changes.
    /// </summary>
    public static async Task<bool> WaitForConditionAsync(
        Func<Task<bool>> condition,
        TimeSpan? timeout = null,
        TimeSpan? pollingInterval = null,
        CancellationToken cancellationToken = default
    )
    {
        timeout ??= TimeSpan.FromSeconds(5);
        pollingInterval ??= TimeSpan.FromMilliseconds(50);

        var deadline = DateTime.UtcNow + timeout.Value;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await condition())
                return true;

            await Task.Delay(pollingInterval.Value, cancellationToken);
        }

        return false;
    }
}
