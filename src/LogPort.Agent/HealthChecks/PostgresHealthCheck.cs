using LogPort.Core.Models;
using LogPort.Internal;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Npgsql;

namespace LogPort.Agent.HealthChecks;

public class PostgresHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public PostgresHealthCheck(LogPortConfig config)
    {
        _connectionString = config.Postgres.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            // Lightweight query to ensure DB is responsive
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Postgres is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres is unavailable", ex);
        }
    }
}