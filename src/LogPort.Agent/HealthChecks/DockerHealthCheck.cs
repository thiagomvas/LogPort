using Docker.DotNet;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogPort.Agent.HealthChecks;

public class DockerHealthCheck : IHealthCheck
{
    private readonly DockerClient _client;

    public DockerHealthCheck(DockerClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await _client.System.GetVersionAsync(cancellationToken);

            return HealthCheckResult.Healthy($"Docker OK: version {version.Version}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cannot reach Docker daemon", ex);
        }
    }
}