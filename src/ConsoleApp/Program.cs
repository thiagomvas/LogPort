using LogPort.Core;
using LogPort.Internal.Services;
var line = """
           INFO something before {"Timestamp":"2025-01-01T12:00:00Z","Level":"warn","Message":"hello world 🚀"}
           """;

var extractor = new JsonLogEntryExtractor(new()
{
    MessageProperty = "Message",
    LevelProperty = "Level",
    TimestampProperty = "Timestamp",
});

if (extractor.TryExtract(line.AsSpan(), out var log))
{
    Console.WriteLine($"Timestamp: {log.Timestamp:o}");
    Console.WriteLine($"Level: {log.Level}");
    Console.WriteLine($"Message: {log.Message}");
}
else
{
    Console.WriteLine("Extraction failed");
}