using System.Text.Json;

using Docker.DotNet;

using LogPort.Agent;
using LogPort.Agent.Endpoints;
using LogPort.Agent.HealthChecks;
using LogPort.Agent.Services;
using LogPort.AspNetCore;
using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Data.Postgres;
using LogPort.Internal;
using LogPort.Internal.Abstractions;
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
builder.Services.AddHttpClient();
bool isAgent = logPortConfig.Mode is LogMode.Agent;

if (isAgent)
    builder.Services.AddLogPortAgent(logPortConfig);
else
    builder.Services.AddLogPortRelay(logPortConfig, builder);


if (logPortConfig.Docker.Use)
{
    builder.Services.AddSingleton(_ =>
        new DockerClientConfiguration(new Uri(logPortConfig.Docker.SocketPath))
            .CreateClient());

    builder.Services.AddHealthChecks().AddCheck<DockerHealthCheck>("docker");
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

var adminUser = Environment.GetEnvironmentVariable("LOGPORT_ADMIN_USER");
var adminPass = Environment.GetEnvironmentVariable("LOGPORT_ADMIN_PASS");

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/analytics"))
    {
        await next();
        return;
    }

    if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrWhiteSpace(adminPass))
    {
        context.Response.StatusCode = 503;
        await context.Response.WriteAsync("Admin credentials are not configured.");
        return;
    }

    var auth = context.Request.Headers.Authorization.ToString();

    if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = 401;
        context.Response.Headers.WWWAuthenticate = "Basic";
        return;
    }

    try
    {
        var encoded = auth["Basic ".Length..].Trim();
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

        var parts = decoded.Split(':', 2);
        if (parts.Length != 2)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.WWWAuthenticate = "Basic";
            return;
        }

        var userOK = parts[0] == adminUser;
        var PassOK = parts[1] == adminPass;

        if (!userOK || !PassOK)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.WWWAuthenticate = "Basic";
            return;
        }
    }
    catch
    {
        context.Response.StatusCode = 400;
        return;
    }

    await next();
});


if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseWebSockets();

if (isAgent)
    app.MapAgentEndpoints();

app.MapSocketEndpoints();


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

app.Lifetime.ApplicationStopping.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var manager = scope.ServiceProvider.GetRequiredService<WebSocketManager>();
    manager.AbortAll();
});


app.Run($"http://0.0.0.0:{logPortConfig.Port}");