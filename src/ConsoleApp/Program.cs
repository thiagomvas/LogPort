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

var now = DateTime.UtcNow;

var last5s = store.QueryCount(
    "logs.processed",
    TimeSpan.FromSeconds(5));

var last30s = store.QueryCount(
    "logs.processed",
    TimeSpan.FromSeconds(30));

Console.WriteLine($"[{now:HH:mm:ss}]Logs in last 5s: {last5s}");
Console.WriteLine($"[{now:HH:mm:ss}]Logs in last 30s: {last30s}");
