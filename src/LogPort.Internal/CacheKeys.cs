namespace LogPort.Internal;

internal static class CacheKeys
{
    public const string LogMetadata = "metadata";
    public const string LogPrefix = "logs_";
    public const string CountPrefix = "count_";
    public const string LogPatterns = "log_patterns";
    
    public static string BuildLogMetadataCacheKey(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        return $"{LogMetadata}:{from?.ToUnixTimeSeconds() ?? 0}:{to?.ToUnixTimeSeconds() ?? 0}";
    }
}