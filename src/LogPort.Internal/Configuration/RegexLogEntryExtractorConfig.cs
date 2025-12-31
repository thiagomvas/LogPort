namespace LogPort.Internal.Configuration;

public sealed class RegexLogEntryExtractorConfig : BaseLogEntryExtractorConfig
{
    public required string Pattern { get; init; }

    public required string MessageGroup { get; init; }
    public required string LevelGroup { get; init; }
    public required string TimestampGroup { get; init; }
}
