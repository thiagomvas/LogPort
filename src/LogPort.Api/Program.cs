using System.Text.Json;
using LogPort.Api.Endpoints;
using LogPort.Api.HealthChecks;
using LogPort.Core.Interface;
using LogPort.Core.Models;
using LogPort.ElasticSearch;
using LogPort.Postgres;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Configuration.AddEnvironmentVariables(prefix: "LOGPORT_");
var logPortConfig = LogPortConfig.LoadFromEnvironment();
builder.Configuration.GetSection("LOGPORT").Bind(logPortConfig);
builder.Services.AddSingleton(logPortConfig);
if (logPortConfig.UseElasticSearch)
{
    builder.Services.AddSingleton(ElasticClientFactory.Create(logPortConfig));
    builder.Services.AddScoped<ILogRepository, ElasticLogRepository>();
    builder.Services.AddHealthChecks()
        .AddCheck<ElasticsearchHealthCheck>("elasticsearch");
}

if (logPortConfig.UsePostgres)
{
    var connectionString = logPortConfig.PostgresConnectionString;
    await DatabaseInitializer.InitializeAsync(connectionString, true);
    builder.Services.AddScoped<ILogRepository>(sp => new PostgresLogRepository(connectionString));
    builder.Services.AddHealthChecks()
        .AddCheck<PostgresHealthCheck>("postgres");
}

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
app.MapLogEndpoints();

app.Map("/stream", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[8192];
    var logRepository = context.RequestServices.GetRequiredService<ILogRepository>();
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: context.RequestAborted);
        if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing", context.RequestAborted);
            break;
        }

        var jsonMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
        try
        {
            var logEntry = JsonSerializer.Deserialize<LogEntry>(jsonMessage);
            if (logEntry != null)
            {
                await logRepository.AddLogAsync(logEntry);
                logger.LogInformation("Received log: {LogEntry}", jsonMessage);
            }
        }
        catch (JsonException)
        {
        }
    }
});

// Run on port 5000
app.Run("http://localhost:5000");