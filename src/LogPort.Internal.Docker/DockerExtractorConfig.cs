namespace LogPort.Internal.Docker;

public class DockerExtractorConfig
{
    public string ServiceName { get; set; } = string.Empty;
    public string ExtractionMode { get; set; } = string.Empty;
    public string? MessageKey { get; set; }
    public string? TimestampKey { get; set; }
    public string? LogLevelKey { get; set; }
    public string? ExtractorRegex { get; set; }
}