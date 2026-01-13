using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Metrics;
using LogPort.Internal.Services;

var pipeline = new LogEntryExtractionPipeline(new LogPortConfig()
{
    Extractors =
    [
        new BaseLogEntryExtractorConfig() { ServiceName = "foo", TemplateKey = "postgres" }
    ]
});

var json = """2026-01-13 17:00:39.366 UTC [1] LOG: database system is ready to accept connections""";

if (pipeline.TryExtract("foo", json, out var entry))
{
    Console.WriteLine(entry.Message);
    Console.WriteLine(entry.Level);
    Console.WriteLine(entry.Timestamp);
}