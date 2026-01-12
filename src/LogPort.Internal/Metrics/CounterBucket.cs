using LogPort.Internal.Abstractions;

namespace LogPort.Internal.Metrics;

internal sealed class CounterBucket : IRollingBucket
{
    public ulong Value;

    public void Increment(ulong value = 1)
    {
        Interlocked.Add(ref Value, value);
    }

    public void Reset()
    {
        Interlocked.Exchange(ref Value, 0);
    }
}