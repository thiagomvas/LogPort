namespace LogPort.Internal.Configuration;

public class MetricsConfig
{
    public TimeSpan BucketDuration { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan MaxWindow { get; set; } = TimeSpan.FromMinutes(15);
}