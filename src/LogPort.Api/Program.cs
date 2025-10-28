using LogPort.Core.Interface;
using LogPort.Core.Models;
using LogPort.ElasticSearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(ElasticClientFactory.Create());
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record LogEntryDto(string Message, DateTime Timestamp, string Level);