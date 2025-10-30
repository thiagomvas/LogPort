using System.Net.Http.Headers;
using LogPort.Core.Models;
using LogPort.Internal.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogPort.Agent.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        app.MapPost("/analytics/histogram", async ([FromServices] AnalyticsService service,
            LogQueryParameters? parameters) =>
        {
            return await GetHistogram(service, parameters);
        })
        .WithTags("Analytics")
        .WithName("GetLogHistogram")
        .WithSummary("Retrieves a histogram of log entries over time based on the provided query parameters.");
    }

    private static async Task<IResult> GetHistogram(AnalyticsService service, LogQueryParameters? parameters)
    {
        parameters ??= new LogQueryParameters();
        var histogram = await service.GetLogHistogramAsync(parameters);

        return Results.Ok(histogram);
    }
    
}