using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

namespace LogPort.Internal.UnitTests.Extractors;

[TestFixture]
public sealed class JsonLogEntryExtractorTests
{
    private static JsonLogEntryExtractor CreateExtractor() =>
        new(new JsonLogEntryExtractorConfig
        {
            MessageProperty = "message",
            LevelProperty = "level",
            TimestampProperty = "timestamp"
        });

    [Test]
    public void TryExtract_ReturnsFalse_WhenNoJson()
    {
        var extractor = CreateExtractor();

        var success = extractor.TryExtract(
            "plain text log".AsSpan(),
            out var entry);

        Assert.That(success, Is.False);
        Assert.That(entry, Is.EqualTo(null));
    }

    [Test]
    public void TryExtract_ReturnsFalse_WhenMessageMissing()
    {
        var extractor = CreateExtractor();

        var success = extractor.TryExtract(
            """{"level":"info"}""".AsSpan(),
            out _);

        Assert.That(success, Is.False);
    }

    [Test]
    public void TryExtract_ExtractsMessageLevelAndTimestamp()
    {
        var extractor = CreateExtractor();

        var success = extractor.TryExtract(
            """{"message":"hello","level":"warn","timestamp":"2024-01-01T00:00:00Z"}""".AsSpan(),
            out var entry);

        Assert.That(success, Is.True);
        Assert.That(entry.Message, Is.EqualTo("hello"));
        Assert.That(entry.Level, Is.EqualTo("WARN"));
        Assert.That(
            entry.Timestamp,
            Is.EqualTo(DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime()));
    }

    [Test]
    public void TryExtract_DefaultsLevelAndTimestamp_WhenMissing()
    {
        var extractor = CreateExtractor();

        var success = extractor.TryExtract(
            """{"message":"hello"}""".AsSpan(),
            out var entry);

        Assert.That(success, Is.True);
        Assert.That(entry.Level, Is.EqualTo("INFO"));
        Assert.That(
            entry.Timestamp,
            Is.InRange(
                DateTime.UtcNow.AddSeconds(-2),
                DateTime.UtcNow.AddSeconds(2)));
    }
}
