using LogPort.Internal.Metrics;

namespace LogPort.Agent.Endpoints;

public static class MetricsEndpoints
{
    public static void MapMetricsEndpoints(this WebApplication app)
    {
        app.MapGet("api/metrics", GetMetricsSnapshotAsync);
    }

    private static Task<IResult> GetMetricsSnapshotAsync(MetricStore metrics)
    {
        var snapshot = metrics.Snapshot();
        return Task.FromResult(Results.Ok(snapshot));
    }
}