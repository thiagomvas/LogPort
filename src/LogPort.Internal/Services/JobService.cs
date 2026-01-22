using Hangfire;
using Hangfire.Storage;

using LogPort.Internal.Abstractions;
using LogPort.Internal.Models;

using Microsoft.Extensions.Logging;

namespace LogPort.Internal.Services;

public sealed class JobService
{
    private readonly IEnumerable<JobBase> _jobs;
    private readonly ILogger<JobService>? _logger;

    public JobService(IEnumerable<JobBase> jobs, ILogger<JobService>? logger = null)
    {
        _jobs = jobs;
        _logger = logger;
    }

    public IEnumerable<JobMetadata> GetMetadata()
    {

        var storage = JobStorage.Current;
        var monitoring = storage.GetMonitoringApi();

        using var connection = storage.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();

        return _jobs.Select(j =>
            {
                var recurring = recurringJobs.FirstOrDefault(rj => rj.Id == j.Id);
                return new JobMetadata
                {
                    Id = j.Id,
                    Name = j.Name,
                    Description = j.Description,
                    LastExecution = recurring?.LastExecution,
                    NextExecution = recurring?.NextExecution,
                    IsEnabled = j.Enabled,
                    Cron = j.Cron
                };
            })
            .Where(x => x != null);
    }

    public void Trigger(string jobId)
    {
        var job = _jobs.FirstOrDefault(j => j.Id == jobId);
        if (job is null)
            throw new InvalidOperationException($"Job '{jobId}' not found");

        var storage = JobStorage.Current;

        using var connection = storage.GetConnection();

        var recurring = connection
            .GetRecurringJobs()
            .Any(r => r.Id == jobId);

        if (recurring)
        {
            RecurringJob.TriggerJob(jobId);
            _logger?.LogInformation("Recurring job '{JobId}' was triggered", jobId);
            return;
        }

        BackgroundJob.Enqueue(() => job.ExecuteAsync());
        _logger?.LogInformation("Job '{JobId}' was triggered", jobId);
    }


}