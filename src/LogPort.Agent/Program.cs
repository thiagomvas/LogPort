using System.Text;
using System.Text.Json;

using Docker.DotNet;

using Hangfire;

using LogPort.Agent;
using LogPort.Agent.Endpoints;
using LogPort.Agent.HealthChecks;
using LogPort.Agent.Middlewares;
using LogPort.Agent.Services;
using LogPort.AspNetCore;
using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Docker;
using LogPort.Internal.Metrics;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.IdentityModel.Tokens;


using WebSocketManager = LogPort.Internal.Services.WebSocketManager;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var logPortConfig = ConfigLoader.Load();
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



var jwtKey = logPortConfig.JwtSecret;
var jwtIssuer = logPortConfig.JwtIssuer ?? "LogPort";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    })
;
builder.Services.AddAuthorization();

builder.Services.AddSingleton<LogQueue>();
builder.Services.AddHostedService<LogBatchProcessor>();
builder.Services.AddSingleton<WebSocketManager>();
builder.Services.AddSingleton<MetricStore>(
    sp => new MetricStore(sp.GetRequiredService<LogPortConfig>()));

builder.Services.AddWebSockets(options => { options.KeepAliveInterval = TimeSpan.FromSeconds(30); });

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (logPortConfig.Mode is LogMode.Relay)
    await app.UseLogPortAsync();

app.UseCors("Default");
app.MapAuthEndpoints(logPortConfig);
app.UseAuthentication();
app.UseAuthorization();


if (!string.IsNullOrWhiteSpace(logPortConfig.ApiSecret))
{
    app.UseWhen(
        ctx => ctx.Request.Path.StartsWithSegments("/agent"),
        branch => branch.UseMiddleware<ApiTokenMiddleware>()
    );

    logger.LogInformation(
        "API token authentication ENABLED for /agent endpoints."
    );
}
else
{
    logger.LogWarning(
        "API token authentication is DISABLED. " +
        "No ApiSecret is configured, so any external application can stream logs to LogPort. " +
        "THIS IS NOT RECOMMENDED FOR PRODUCTION."
    );
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHangfireDashboard("/hangfire");
}

app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
    await next();
});


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

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { WriteIndented = true }));
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var manager = scope.ServiceProvider.GetRequiredService<WebSocketManager>();
    manager.AbortAll();
});


app.Run($"http://0.0.0.0:{logPortConfig.Port}");