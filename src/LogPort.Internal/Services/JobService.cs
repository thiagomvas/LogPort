using Hangfire;
using Hangfire.Storage;

using LogPort.Internal.Abstractions;
using LogPort.Internal.Models;

namespace LogPort.Internal.Services;

public sealed class JobService
{
    private readonly IEnumerable<JobBase> _jobs;

    public JobService(IEnumerable<JobBase> jobs)
    {
        _jobs = jobs;
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
                    IsEnabled = recurring is not null,
                    Cron = j.Cron
                };
            })
            .Where(x => x != null);
    }

}
