using System.Text.Json.Serialization;

namespace LogPort.Internal.Configuration;


[JsonConverter(typeof(BaseLogEntryExtractorConfigJsonConverter))]
public class BaseLogEntryExtractorConfig
{
    public string ServiceName { get; init; } = string.Empty;
    public string ExtractionMode { get; init; } = string.Empty;
    public string? TemplateKey { get; init; } = null;
}