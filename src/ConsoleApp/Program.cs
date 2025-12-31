using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Services;

var json = """
           {
             "Port": 8080,
             "Mode": "Agent",

             "Postgres": {
               "Use": true
             },

             "Extractors": [
               {
                 "ServiceName": "example-service",
                 "ExtractionMode": "Json",

                 "MessageProperty": "Message",
                 "LevelProperty": "Level",
                 "TimestampProperty": "Timestamp"
               }
             ]
           }
           """;

// Load config from JSON
var config = ConfigLoader.LoadFromJson(json);

// Build extractor pipeline ONCE
var pipeline = new LogEntryExtractionPipeline(config.Extractors);

// Example log line
var line = """
           INFO something before {"Timestamp":"2025-01-01T12:00:00Z","Level":"warn","Message":"hello world 🚀"}
           """;

// Service name MUST match config
if (pipeline.TryExtract("example-service", line.AsSpan(), out var log))
{
    Console.WriteLine($"Timestamp: {log.Timestamp:o}");
    Console.WriteLine($"Level: {log.Level}");
    Console.WriteLine($"Message: {log.Message}");
}
else
{
    Console.WriteLine("Extraction failed");
}