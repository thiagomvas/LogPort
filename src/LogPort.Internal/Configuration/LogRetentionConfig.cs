namespace LogPort.Internal.Configuration;

public sealed class LogRetentionConfig
{
    public string AutomaticCleanupCron { get; set; } = "0 3 * * *";
    public int RetentionDays { get; set; } = 14;
    public bool EnableAutomaticCleanupJob { get; set; } = true;
}