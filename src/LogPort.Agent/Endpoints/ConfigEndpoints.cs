using Hangfire;
using Hangfire.Storage;

using LogPort.Agent.Models;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

using Microsoft.AspNetCore.Mvc;

namespace LogPort.Agent.Endpoints;

public static class ConfigEndpoints
{
    public static void MapConfigEndpoints(this WebApplication app)
    {
        app.MapGet("/api/config", GetConfig)
            .RequireAuthorization();

        app.MapPost("/api/config", SaveConfig);
    }

    private static Task<IResult> GetConfig(LogPortConfig config)
    {
        return Task.FromResult(Results.Ok(config));
    }

    public static Task<IResult> SaveConfig([FromBody] LogPortConfig config)
    {
        ConfigLoader.SaveToFile(config);
        return Task.FromResult(Results.Ok());
    }


}