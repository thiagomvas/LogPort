using LogPort.AspNetCore;
using LogPort.SDK;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Logging.AddConsole();
builder.AddLogPort(options =>
{
    options.AgentUrl = "ws://localhost:8080/";
    options.ServiceName = "logport-testapi";
    options.ApiSecret = "secret";
});

builder.Logging.SetMinimumLevel(LogLevel.Debug);

// IMPORTANT: category filters
builder.Logging.AddFilter("Microsoft", LogLevel.Debug);
builder.Logging.AddFilter("System", LogLevel.Debug);
builder.Logging.AddFilter("LogPort", LogLevel.Debug);
builder.Logging.AddFilter("Default", LogLevel.Debug);
var app = builder.Build();

await app.UseLogPortAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
    {
        logger.LogError("Generating weather forecast");
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        logger.LogWarning("Generated forecast: {@Forecast}", forecast);
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}