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
        app.MapFallbackToFile("index.html");
        var config = app.Services.GetRequiredService<LogPortConfig>();
        app.ConfigureJobs(config);
    }

    public static void ConfigureJobs(this WebApplication app, LogPortConfig config)
    {
        if (config.Retention.EnableAutomaticCleanupJob)
        {
            RecurringJob.AddOrUpdate<LogPartitionCleanupJob>(
                "log-partition-cleanup",
                j => j.ExecuteAsync(),
                config.Retention.AutomaticCleanupCron);
        }
        
    }
}