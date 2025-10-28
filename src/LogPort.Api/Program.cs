using System.Text.Json;
using LogPort.Api.Endpoints;
using LogPort.Api.HealthChecks;
using LogPort.Core.Interface;
using LogPort.Core.Models;
using LogPort.ElasticSearch;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Configuration.AddEnvironmentVariables(prefix: "LOGPORT_");
var logPortConfig = LogPortConfig.LoadFromEnvironment();
builder.Configuration.GetSection("LOGPORT").Bind(logPortConfig);
builder.Services.AddSingleton(logPortConfig);
builder.Services.AddSingleton(ElasticClientFactory.Create(logPortConfig));
builder.Services.AddScoped<ILogRepository, ElasticLogRepository>();
builder.Services.AddHealthChecks()
    .AddCheck<ElasticsearchHealthCheck>("elasticsearch");
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

app.MapLogEndpoints();

app.Run();