namespace LogPort.Agent.Models;

public sealed record RecurringJobStatusDto(
    string Id,
    string Cron,
    DateTime? LastExecution,
    DateTime? NextExecution
);
