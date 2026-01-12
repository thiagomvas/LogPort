namespace LogPort.Internal.Metrics;

public readonly struct CounterSnapshot
{
    public ulong? Last1s { get; }
    public ulong? Last10s { get; }
    public ulong? Last1m { get; }

    public ulong[]? Buckets { get; }

    public CounterSnapshot(ulong last1s, ulong last10s, ulong last1m)
    {
        Last1s = last1s;
        Last10s = last10s;
        Last1m = last1m;
        Buckets = null;
    }

    public CounterSnapshot(ulong[] buckets)
    {
        Last1s = 0;
        Last10s = 0;
        Last1m = 0;
        Buckets = buckets;
    }
}