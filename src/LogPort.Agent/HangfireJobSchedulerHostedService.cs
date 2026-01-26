using Hangfire;

using LogPort.Internal.Abstractions;

namespace LogPort.Agent;

public sealed class HangfireJobSchedulerHostedService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceProvider _serviceProvider;

    public HangfireJobSchedulerHostedService(
        IRecurringJobManager recurringJobManager,
        IServiceProvider serviceProvider)
    {
        _recurringJobManager = recurringJobManager;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var jobs = scope.ServiceProvider.GetServices<JobBase>();

        foreach (var job in jobs)
        {
            _recurringJobManager.AddOrUpdate(
                job.Id,
                () => job.ExecuteAsync(),
                job.Cron
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}