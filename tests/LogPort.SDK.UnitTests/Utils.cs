namespace LogPort.SDK.UnitTests;

public static class Utils
{
    public static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000, int pollIntervalMs = 25)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                throw new TimeoutException("Condition not met within timeout.");
            await Task.Delay(pollIntervalMs);
        }
    }
}
