using Hangfire;
using Hangfire.Storage;

using LogPort.Agent.Models;
using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

namespace LogPort.Agent.Endpoints;

public static class ConfigEndpoints
{
    public static void MapConfigEndpoints(this WebApplication app)
    {
        app.MapGet("/api/config", GetConfig)
            .RequireAuthorization();
    }

    private static Task<IResult> GetConfig(LogPortConfig config)
    {
        return Task.FromResult(Results.Ok(config));
    }


}