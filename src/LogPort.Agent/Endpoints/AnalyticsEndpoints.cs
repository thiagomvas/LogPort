using System.Net.Http.Headers;
using LogPort.Core.Models;
using LogPort.Internal.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogPort.Agent.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        app.MapPost("/analytics/histogram", GetHistogram)
        .WithTags("Analytics")
        .WithName("GetLogHistogram")
        .WithSummary("Retrieves a histogram of log entries over time based on the provided query parameters.");

        app.MapPost("/analytics/count-by-type", GetCountByType);
    }

    private static async Task<IResult> GetHistogram(AnalyticsService service, LogQueryParameters? parameters, [FromQuery] TimeSpan? interval)
    {
        parameters ??= new LogQueryParameters();
        interval ??= TimeSpan.FromHours(1);
        var histogram = await service.GetLogHistogramAsync(parameters, interval);

        return Results.Ok(histogram);
    }

    public static async Task<IResult> GetCountByType(AnalyticsService service, LogQueryParameters? parameters)
    {
        parameters ??= new LogQueryParameters();
        var counts = await service.GetCountByTypeAsync(parameters);

        return Results.Ok(counts);
    }
    
}