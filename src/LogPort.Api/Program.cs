using LogPort.Core.Interface;
using LogPort.Core.Models;
using LogPort.ElasticSearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Configuration.AddEnvironmentVariables(prefix: "LOGPORT_");
var logPortConfig = LogPortConfig.LoadFromEnvironment();
builder.Configuration.GetSection("LOGPORT").Bind(logPortConfig);
builder.Services.AddSingleton(logPortConfig);
builder.Services.AddSingleton(ElasticClientFactory.Create(logPortConfig));
builder.Services.AddScoped<ILogRepository, ElasticLogRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/logs", async (ILogRepository logRepository, LogEntryDto logDto) =>
{
    var logEntry = new LogEntry
    {
        Message = logDto.Message,
        Timestamp = logDto.Timestamp,
        Level = logDto.Level
    };

    await logRepository.AddLogAsync(logEntry);
    return Results.Created($"/logs/{logEntry.Timestamp.Ticks}", logEntry);
});

app.MapGet("/logs", async (ILogRepository logRepository, DateTime? from, DateTime? to, string? level) =>
{
    var logs = await logRepository.GetLogsAsync(from, to, level);
    return Results.Ok(logs);
});

record LogEntryDto(string Message, DateTime Timestamp, string Level);