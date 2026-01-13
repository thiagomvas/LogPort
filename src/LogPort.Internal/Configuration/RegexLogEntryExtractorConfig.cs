namespace LogPort.Internal.Configuration;

public sealed class RegexLogEntryExtractorConfig : BaseLogEntryExtractorConfig
{
    public required string Pattern { get; init; }

    public required string MessageGroup { get; init; }
    public string LevelGroup { get; init; }
    public string TimestampGroup { get; init; }

    public RegexLogEntryExtractorConfig()
    {
        ExtractionMode = "regex";
    }
}