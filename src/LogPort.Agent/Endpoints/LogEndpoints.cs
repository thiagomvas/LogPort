using System.Text;
using System.Text.Json;

using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Services;

using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Logging;

using WebSocketManager = LogPort.Internal.Services.WebSocketManager;

namespace LogPort.Agent.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this WebApplication app)
    {
        app.MapPost("api/logs", AddLogAsync);

        app.MapGet("api/logs", GetLogsAsync);
        app.MapGet("api/logs/count", CountLogsAsync);
        app.MapGet("api/logs/metadata", GetLogMetadataAsync);
        app.MapGet("api/logs/query", QueryLogsAsync);
    }

    private static async Task<IResult> AddLogAsync(LogService logRepository, LogEntry log)
    {
        await logRepository.AddLogAsync(log);
        return Results.Created($"api/logs", log);
    }

    private static async Task<IResult> GetLogsAsync(
        LogService logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var logs = await logRepository.GetLogsAsync(parameters);
        return Results.Ok(logs);
    }
    private static async Task<IResult> CountLogsAsync(
        LogService logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var count = await logRepository.CountLogsAsync(parameters);
        return Results.Ok(new { Count = count });
    }

    private static async Task<IResult> GetLogMetadataAsync(
        LogService repository,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? lastDays)
    {
        if (lastDays.HasValue && lastDays > 0)
        {
            to ??= DateTimeOffset.UtcNow;
            from = to.Value.AddDays(-lastDays.Value);
        }

        var metadata = await repository.GetLogMetadataAsync(from, to);
        return Results.Ok(metadata);
    }

    private static async Task<IResult> QueryLogsAsync(
        LogService logService,
        string? query,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 100)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Results.BadRequest("Query string is required.");

        var result = await logService.QueryLogsAsync(
            query,
            from,
            to,
            page,
            pageSize
        );

        return Results.Ok(result);
    }

}