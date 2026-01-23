namespace LogPort.Internal.Configuration;

public sealed class LevelRetentionConfig
{
    public string AutomaticCleanupCron { get; set; } = "0 3 * * *";
    public Dictionary<string, TimeSpan> Retentions { get; set; } = [];
    public bool EnableAutomaticCleanupJob { get; set; } = true;
}