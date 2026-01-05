using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Metrics;
using LogPort.Internal.Services;

var store = new MetricStore(TimeSpan.FromSeconds(1),TimeSpan.FromMinutes(1));

store.GetOrRegisterCounter("logs.processed");

for (int i = 0; i < 30; i++)
{
    store.Increment("logs.processed");

    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] processed log");

    Thread.Sleep(Random.Shared.Next(100, 1500));
}

var dump = store.Snapshot();

Console.WriteLine($"Metrics dump at {dump.TimestampUtc:O}");

foreach (var (name, counter) in dump.Counters)
{
    Console.WriteLine(
        $"{name} | " +
        $"1s={counter.Last1s}, " +
        $"10s={counter.Last10s}, " +
        $"1m={counter.Last1m}");
}