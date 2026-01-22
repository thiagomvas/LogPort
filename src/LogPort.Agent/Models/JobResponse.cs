namespace LogPort.Agent.Models;

public sealed class JobResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime LastExecution { get; set; }
    public DateTime NextExecution { get; set; }
    public bool IsEnabled { get; set; }
    public string Cron { get; set; }
}