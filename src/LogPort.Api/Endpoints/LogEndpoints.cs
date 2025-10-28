using LogPort.Core.Interface;
using LogPort.Core.Models;

namespace LogPort.Api.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this WebApplication app)
    {
        // Basic log ingestion
        app.MapPost("/logs", AddLogAsync);

        // Simple GET search via query string
        app.MapGet("/logs", GetLogsAsync);

        // Complex search via POST with full LogQueryParameters
        app.MapPost("/logs/search", SearchLogsAsync);
    }

    // Add a new log
    private static async Task<IResult> AddLogAsync(ILogRepository logRepository, LogEntry log)
    {
        await logRepository.AddLogAsync(log);
        return Results.Created($"/logs", log);
    }

    // GET /logs?From=...&To=...&Level=...&Search=...
    private static async Task<IResult> GetLogsAsync(
        ILogRepository logRepository,
        DateTime? From,
        DateTime? To,
        string? Level,
        string? Search,
        bool SearchExact = true,
        int Page = 1,
        int PageSize = 100)
    {
        var parameters = new LogQueryParameters
        {
            From = From,
            To = To,
            Level = Level,
            Search = Search,
            SearchExact = SearchExact,
            Page = Page,
            PageSize = PageSize
        };

        var logs = await logRepository.GetLogsAsync(parameters);
        return Results.Ok(logs);
    }

    private static async Task<IResult> SearchLogsAsync(
        ILogRepository logRepository,
        LogQueryParameters parameters)
    {
        var logs = await logRepository.GetLogsAsync(parameters);
        return Results.Ok(logs);
    }
}