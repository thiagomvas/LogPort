using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

namespace LogPort.Internal.UnitTests.Extractors;

[TestFixture]
public sealed class LogEntryExtractionPipelineTests
{
    [Test]
    public void TryExtract_ReturnsFalse_WhenServiceNotConfigured()
    {
        var pipeline = new LogEntryExtractionPipeline(
            new LogPortConfig { Extractors = [] });

        Assert.That(
            pipeline.TryExtract("unknown", "log".AsSpan(), out _),
            Is.False);
    }

    [Test]
    public void TryExtract_UsesCorrectExtractor_ByServiceName()
    {
        var config = new LogPortConfig
        {
            Extractors =
            [
                new JsonLogEntryExtractorConfig
                {
                    ServiceName = "api",
                    ExtractionMode = "json",
                    MessageProperty = "msg",
                    LevelProperty = "lvl",
                    TimestampProperty = "ts"
                }
            ]
        };

        var pipeline = new LogEntryExtractionPipeline(config);

        var success = pipeline.TryExtract(
            "API",
            """{"msg":"hello","lvl":"info"}""".AsSpan(),
            out var entry);

        Assert.That(success, Is.True);
        Assert.That(entry.Message, Is.EqualTo("hello"));
    }

    [Test]
    public void Constructor_Throws_WhenExtractorModeUnknown()
    {
        var config = new LogPortConfig
        {
            Extractors =
            [
                new BaseLogEntryExtractorConfig
                {
                    ServiceName = "api",
                    ExtractionMode = "wat"
                }
            ]
        };

        Assert.That(
            () => new LogEntryExtractionPipeline(config),
            Throws.TypeOf<InvalidOperationException>());
    }
}