using LogPort.Internal;
using LogPort.Internal.Configuration;

namespace LogPort.Agent.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

public class UpstreamHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly LogPortConfig _config;

    public UpstreamHealthCheck(HttpClient httpClient, LogPortConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.UpstreamUrl))
            return HealthCheckResult.Unhealthy("UpstreamUrl is not configured");

        try
        {
            var response = await _httpClient.GetAsync($"{_config.UpstreamUrl}/health", cancellationToken);

            if (response.IsSuccessStatusCode)
                return HealthCheckResult.Healthy("Upstream reachable");

            return HealthCheckResult.Unhealthy($"Upstream returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to reach upstream", ex);
        }
    }
}