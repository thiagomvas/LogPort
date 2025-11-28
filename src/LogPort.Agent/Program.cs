using System.Text.Json;
using LogPort.Agent.Endpoints;
using LogPort.Agent.HealthChecks;
using LogPort.Agent.Services;
using LogPort.AspNetCore;
using LogPort.Core;
using LogPort.Internal.Abstractions;
using LogPort.Core.Models;
using LogPort.Internal.ElasticSearch;
using LogPort.Data.Postgres;
using LogPort.Internal;
using LogPort.Internal.Common.Services;
using LogPort.Internal.Docker;
using LogPort.Internal.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.WebSockets;
using StackExchange.Redis;
using WebSocketManager = LogPort.Internal.Common.Services.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Configuration.AddEnvironmentVariables(prefix: "LOGPORT_");
var logPortConfig = LogPortConfig.LoadFromEnvironment();
builder.Configuration.GetSection("LOGPORT").Bind(logPortConfig);
builder.Services.AddSingleton(logPortConfig);

bool isAgent = logPortConfig.Mode == LogMode.Agent;
if (isAgent)
{
    if (logPortConfig.Elastic.Use)
    {
        builder.Services.AddSingleton(ElasticClientFactory.Create(logPortConfig));
        builder.Services.AddScoped<ILogRepository, ElasticLogRepository>();
        builder.Services.AddHealthChecks()
            .AddCheck<ElasticsearchHealthCheck>("elasticsearch");
    }

    if (logPortConfig.Postgres.Use)
    {
        builder.Services.AddScoped<ILogRepository, PostgresLogRepository>();
        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres");

        builder.Services.AddHostedService<PostgresInitializerHostedService>();
    }

    if (logPortConfig.Cache.UseRedis)
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(logPortConfig.Cache.RedisConnectionString ??
                                          throw new InvalidOperationException(
                                              "Redis connection string is not configured")));

        builder.Services.AddScoped<ICache, RedisCacheAdapter>();
    }

    else
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ICache, InMemoryCacheAdapter>();
    }

    builder.Services.AddScoped<AnalyticsService>();
    builder.Services.AddSingleton<LogNormalizer>();
    builder.Services.AddScoped<LogService>();

    builder.Services.AddScoped<ILogBatchHandler, AgentLogBatchHandler>();
}
else
{
    builder.AddLogPort(o =>
    {
        o.AgentUrl = logPortConfig.UpstreamUrl ??
                     throw new InvalidOperationException("UpstreamUrl must be set in Relay mode");
    });
    builder.Services.AddScoped<ILogBatchHandler, RelayLogBatchHandler>();
}

if (logPortConfig.Docker.Use)
{
    builder.Services.AddHostedService<DockerLogService>();
}
builder.Services.AddSingleton<LogQueue>();
builder.Services.AddHostedService<LogBatchProcessor>();
builder.Services.AddSingleton<WebSocketManager>();


builder.Services.AddWebSockets(options => { options.KeepAliveInterval = TimeSpan.FromSeconds(30); });


var app = builder.Build();

if (logPortConfig.Mode is LogMode.Relay)
    await app.UseLogPortAsync();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.UseWebSockets();

app.UseDefaultFiles();
app.UseStaticFiles();

if (isAgent)
{app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalChecks = report.Entries.Count,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    exception = e.Value.Exception?.Message,
                    duration = e.Value.Duration.ToString()
                })
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    });
    app.MapLogEndpoints();
    app.MapAnalyticsEndpoints();
}


app.MapFallbackToFile("index.html");
app.Run($"http://0.0.0.0:{logPortConfig.Port}");