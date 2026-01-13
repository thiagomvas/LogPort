namespace LogPort.Internal.Configuration;

public class JsonLogEntryExtractorConfig : BaseLogEntryExtractorConfig
{
    public required string MessageProperty { get; init; }
    public required string LevelProperty { get; init; }
    public required string TimestampProperty { get; init; }

    public JsonLogEntryExtractorConfig()
    {
        ExtractionMode = "json";
    }
}