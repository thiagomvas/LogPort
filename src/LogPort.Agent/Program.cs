using System.Text.Json;
using LogPort.Agent.Endpoints;
using LogPort.Agent.HealthChecks;
using LogPort.Agent.Services;
using LogPort.Core;
using LogPort.Internal.Common.Interface;
using LogPort.Core.Models;
using LogPort.Internal.ElasticSearch;
using LogPort.Data.Postgres;
using LogPort.Internal.Common.Services;
using LogPort.Internal.Docker;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using WebSocketManager = LogPort.Internal.Common.Services.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Configuration.AddEnvironmentVariables(prefix: "LOGPORT_");
var logPortConfig = LogPortConfig.LoadFromEnvironment();
builder.Configuration.GetSection("LOGPORT").Bind(logPortConfig);
builder.Services.AddSingleton(logPortConfig);

if (logPortConfig.Elastic.Use)
{
    builder.Services.AddSingleton(ElasticClientFactory.Create(logPortConfig));
    builder.Services.AddScoped<ILogRepository, ElasticLogRepository>();
    builder.Services.AddHealthChecks()
        .AddCheck<ElasticsearchHealthCheck>("elasticsearch");
}

if (logPortConfig.Postgres.Use)
{
    var connectionString = logPortConfig.Postgres.ConnectionString;
    builder.Services.AddScoped<ILogRepository, PostgresLogRepository>();
    builder.Services.AddHealthChecks()
        .AddCheck<PostgresHealthCheck>("postgres");
    
    builder.Services.AddHostedService<PostgresInitializerHostedService>();
}

if (logPortConfig.Docker.Use)
{
    builder.Services.AddHostedService<DockerLogService>();
}
builder.Services.AddSingleton<LogQueue>();
builder.Services.AddHostedService<LogBatchProcessor>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddSingleton<WebSocketManager>();

builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health", new HealthCheckOptions
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
app.UseWebSockets();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapLogEndpoints();
app.MapAnalyticsEndpoints();


app.Run($"http://0.0.0.0:{logPortConfig.Port}");
