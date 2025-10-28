using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;

namespace LogPort.Api.HealthChecks;

public class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly ElasticClient _client;

    public ElasticsearchHealthCheck(ElasticClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pingResponse = await _client.PingAsync(ct: cancellationToken);

            if (pingResponse.IsValid)
                return HealthCheckResult.Healthy("Elasticsearch is reachable.");

            return HealthCheckResult.Unhealthy("Elasticsearch ping failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Elasticsearch check threw an exception.", ex);
        }
    }
}