using Hangfire;
using Hangfire.Storage;

using LogPort.Agent.Models;

namespace LogPort.Agent.Endpoints;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        app.MapGet("/api/jobs/recurring", GetRecurringJobsAsync)
            .RequireAuthorization();
    }

    private static Task<IResult> GetRecurringJobsAsync()
    {
        var storage = JobStorage.Current;
        var monitoring = storage.GetMonitoringApi();

        using var connection = storage.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();

        var processingJobs = monitoring.ProcessingJobs(0, int.MaxValue);

        var result = recurringJobs.Select(job => new RecurringJobStatusDto(
            Id: job.Id,
            Cron: job.Cron,
            LastExecution: job.LastExecution,
            NextExecution: job.NextExecution
        ));

        return Task.FromResult(Results.Ok(result));
    }
}