using LogPort.Agent.HealthChecks;
using LogPort.Agent.Services;
using LogPort.AspNetCore;
using LogPort.Core;
using LogPort.Data.Postgres;
using LogPort.Internal;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Common.Services;
using LogPort.Internal.Redis;
using LogPort.Internal.Services;

using StackExchange.Redis;

namespace LogPort.Agent;

public static class ServiceCollectionExtensions
{
    public static void AddLogPortAgent(this IServiceCollection services, LogPortConfig config)
    {
        if (config.Postgres.Use)
        {
            services.AddScoped<ILogRepository, PostgresLogRepository>();
            services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres");
            services.AddHostedService<PostgresInitializerHostedService>();
        }

        if (config.Cache.UseRedis)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(
                    config.Cache.RedisConnectionString ?? throw new InvalidOperationException()
                ));
            services.AddScoped<ICache, RedisCacheAdapter>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddScoped<ICache, InMemoryCacheAdapter>();
        }

        services.AddScoped<AnalyticsService>();
        services.AddSingleton<LogNormalizer>();
        services.AddScoped<LogService>();
        services.AddScoped<ILogBatchHandler, AgentLogBatchHandler>();

        services.AddSingleton<LogEntryExtractionPipeline>();
    }

    public static void AddLogPortRelay(this IServiceCollection services, LogPortConfig config, WebApplicationBuilder builder)
    {
        builder.AddLogPort(o =>
        {
            o.AgentUrl = config.UpstreamUrl ??
                         throw new InvalidOperationException("UpstreamUrl must be set in Relay mode");
        });

        services.AddScoped<ILogBatchHandler, RelayLogBatchHandler>();
        services.AddHealthChecks().AddCheck<UpstreamHealthCheck>("upstream_agent");
    }
}