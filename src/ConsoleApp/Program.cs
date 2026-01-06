using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Metrics;
using LogPort.Internal.Services;
var store = new MetricStore(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));

store.Observe("latency", 120);
store.Observe("latency", 75);

var snapshot = store.Snapshot();

var latency = snapshot.Histograms["latency"];
for (int i = 0; i < latency.Counts.Length; i++)
{
    if (i < latency.Boundaries.Length)
        Console.WriteLine($"<= {latency.Boundaries[i]}: {latency.Counts[i]}");
    else
        Console.WriteLine($"> {latency.Boundaries[^1]}: {latency.Counts[i]}");
}
