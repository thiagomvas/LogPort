namespace LogPort.Internal.Metrics;

internal sealed class Bucket
{
    public ulong Counter;

    public void Increment(ulong value = 1)
    {
        Interlocked.Add(ref Counter, value);
    }

    public void Reset()
    {
        Interlocked.Exchange(ref Counter, 0);
    }
}