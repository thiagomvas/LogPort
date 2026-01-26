using System.Text.Json;

using Hangfire;

using LogPort.Agent.Endpoints;
using LogPort.Data.Postgres;
using LogPort.Internal.Configuration;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LogPort.Agent;

public static class ApplicationBuilderExtensions
{
    public static void MapAgentEndpoints(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapLogEndpoints();
        app.MapAnalyticsEndpoints();
        app.MapPatternEndpoints();
        app.MapMetricsEndpoints();
        app.MapJobEndpoints();
        app.MapFallbackToFile("index.html");
        var config = app.Services.GetRequiredService<LogPortConfig>();
    }

    public static void ConfigureJobs(this WebApplication app, LogPortConfig config)
    {
        if (config.Retention.EnableAutomaticCleanupJob)
        {
            RecurringJob.AddOrUpdate<LogPartitionCleanupJob>(
                LogPartitionCleanupJob.JobId,
                j => j.ExecuteAsync(),
                config.Retention.AutomaticCleanupCron);
        }

        if (config.LevelRetention.EnableAutomaticCleanupJob)
        {
            RecurringJob.AddOrUpdate<LogLevelCleanupJob>(
                LogLevelCleanupJob.JobId,
                j => j.ExecuteAsync(),
                config.LevelRetention.AutomaticCleanupCron);
        }

    }
}