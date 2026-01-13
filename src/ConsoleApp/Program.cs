using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Metrics;
using LogPort.Internal.Services;

var pipeline = new LogEntryExtractionPipeline(new LogPortConfig()
{
    Extractors =
    [
        new BaseLogEntryExtractorConfig() { ServiceName = "foo", TemplateKey = "test" }
    ]
});

var json = """{ "message": "Hello world", "timestamp": "2026-01-13 17:05:39.436", "level":"Warn" }""";

if (pipeline.TryExtract("foo", json, out var entry))
{
    Console.WriteLine(entry.Message);
    Console.WriteLine(entry.Level);
    Console.WriteLine(entry.Timestamp);
}