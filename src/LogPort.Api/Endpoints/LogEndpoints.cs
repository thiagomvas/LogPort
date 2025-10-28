using LogPort.Core.Interface;
using LogPort.Core.Models;

namespace LogPort.Api.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this WebApplication app)
    {
        app.MapPost("/logs", AddLogAsync);
        app.MapGet("/logs", GetLogsAsync);
    }
    
    private static async Task<IResult> AddLogAsync(ILogRepository logRepository, LogEntry log)
    {

        await logRepository.AddLogAsync(log);
        return Results.Created($"/logs", log);
    }
    
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
}