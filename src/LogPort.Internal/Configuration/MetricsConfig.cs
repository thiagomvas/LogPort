namespace LogPort.Internal.Configuration;

public class MetricsConfig
{
    public TimeSpan BucketDuration { get; set; }
    public TimeSpan MaxWindow { get; set; }
}