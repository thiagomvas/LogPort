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
        
        app.MapPost("/api/jobs/{id}/trigger", TriggerJobAsync)
            .RequireAuthorization();

    }

    private static Task<IResult> GetRecurringJobsAsync(JobService service)
    {
        return Task.FromResult(Results.Ok(service.GetMetadata()));
    }
    
    private static Task<IResult> TriggerJobAsync(
        string id,
        JobService service)
    {
        service.Trigger(id);
        return Task.FromResult(Results.NoContent());
    }

}