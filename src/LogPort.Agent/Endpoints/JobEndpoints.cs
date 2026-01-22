using Hangfire;
using Hangfire.Storage;

using LogPort.Agent.Models;
using LogPort.Internal.Services;

namespace LogPort.Agent.Endpoints;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        app.MapGet("/api/jobs", GetRecurringJobsAsync)
            .RequireAuthorization();
    }

    private static Task<IResult> GetRecurringJobsAsync(JobService service)
    {
        return Task.FromResult(Results.Ok(service.GetMetadata()));
    }
}