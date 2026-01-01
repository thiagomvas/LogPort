using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

namespace LogPort.Internal.UnitTests.Extractors;

[TestFixture]
public sealed class RegexLogEntryExtractorTests
{
    private static RegexLogEntryExtractor CreateExtractor() =>
        new(new RegexLogEntryExtractorConfig
        {
            Pattern = @"\[(?<level>\w+)\]\s(?<message>.+?)\s\((?<ts>.+?)\)",
            MessageGroup = "message",
            LevelGroup = "level",
            TimestampGroup = "ts"
        });

    [Test]
    public void TryExtract_ReturnsFalse_WhenNoMatch()
    {
        var extractor = CreateExtractor();

        var success = extractor.TryExtract(
            "invalid log".AsSpan(),
            out _);

        Assert.That(success, Is.False);
    }

    [Test]
    public void TryExtract_ReturnsFalse_WhenMessageGroupMissing()
    {
        var extractor = new RegexLogEntryExtractor(
            new RegexLogEntryExtractorConfig
            {
                Pattern = @"\[(?<level>\w+)\]",
                MessageGroup = "message",
                LevelGroup = "level",
                TimestampGroup = "ts"
            });

        Assert.That(
            extractor.TryExtract("[INFO]".AsSpan(), out _),
            Is.False);
    }

    [Test]
    public void TryExtract_ExtractsAllFields()
    {
        var extractor = CreateExtractor();

        var success = extractor.TryExtract(
            "[error] something broke (2024-01-01T00:00:00Z)".AsSpan(),
            out var entry);

        Assert.That(success, Is.True);
        Assert.That(entry.Message, Is.EqualTo("something broke"));
        Assert.That(entry.Level, Is.EqualTo("ERROR"));
        Assert.That(
            entry.Timestamp,
            Is.EqualTo(DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime()));
    }

    [Test]
    public void TryExtract_DefaultsLevelAndTimestamp_WhenMissing()
    {
        var extractor = new RegexLogEntryExtractor(
            new RegexLogEntryExtractorConfig
            {
                Pattern = @"(?<message>.+)",
                MessageGroup = "message",
                LevelGroup = "level",
                TimestampGroup = "ts"
            });

        var success = extractor.TryExtract("hello".AsSpan(), out var entry);

        Assert.That(success, Is.True);
        Assert.That(entry.Level, Is.EqualTo("INFO"));
        Assert.That(
            entry.Timestamp,
            Is.InRange(
                DateTime.UtcNow.AddSeconds(-2),
                DateTime.UtcNow.AddSeconds(2)));
    }
}