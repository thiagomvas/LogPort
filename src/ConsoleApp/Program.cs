using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Metrics;
using LogPort.Internal.Services;

var processed = new Metric(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30));

Parallel.For(0, 500, i =>
{
    Thread.Sleep(Random.Shared.Next(100, 500));
    processed.Increment();
});

Console.WriteLine(
    $"Logs processed in last 5s: " +
    processed.QueryCount(TimeSpan.FromSeconds(5)));

Console.WriteLine(
    $"Logs processed in last 15s: " +
    processed.QueryCount(TimeSpan.FromSeconds(15)));