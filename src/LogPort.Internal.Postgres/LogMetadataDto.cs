using System.Text.Json;

namespace LogPort.Data.Postgres;

public class LogMetadataDto
{
    public string[]? Levels { get; set; }
    public string[]? Environments { get; set; }
    public string[]? Services { get; set; }
    public string[]? Hostnames { get; set; }
    public long LogCount { get; set; }
    public string? LogCountByLevel { get; set; }
    public string? LogCountByService { get; set; }
    public string? LogCountByEnvironment { get; set; }
    public string? LogCountByHostname { get; set; }

    public LogPort.Core.Models.LogMetadata ToMetadata()
    {
        return new LogPort.Core.Models.LogMetadata
        {
            LogLevels = Levels ?? Array.Empty<string>(),
            Environments = Environments ?? Array.Empty<string>(),
            Services = Services ?? Array.Empty<string>(),
            Hostnames = Hostnames ?? Array.Empty<string>(),
            LogCount = LogCount,
            LogCountByLevel = string.IsNullOrEmpty(LogCountByLevel)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(LogCountByLevel)!,
            LogCountByService = string.IsNullOrEmpty(LogCountByService)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(LogCountByService)!,
            LogCountByEnvironment = string.IsNullOrEmpty(LogCountByEnvironment)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(LogCountByEnvironment)!,
            LogCountByHostname = string.IsNullOrEmpty(LogCountByHostname)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(LogCountByHostname)!
        };
    }
}