using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Services;

var json = """
           {
             "Port": 9000,
             "Mode": "Agent",

             "Extractors": [
               {
                 "ServiceName": "auth-service",
                 "ExtractionMode": "Regex",

                 "Pattern": "(?<ts>\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}Z)\\s+\\[(?<lvl>\\w+)\\]\\s+(?<msg>.+)",
                 "TimestampGroup": "ts",
                 "LevelGroup": "lvl",
                 "MessageGroup": "msg"
               }
             ]
           }
           """;



var config = ConfigLoader.LoadFromJson(json);

var pipeline = new LogEntryExtractionPipeline(config.Extractors);

var line = """
           2024-12-31T23:59:59Z [error] invalid credentials
           """;


if (pipeline.TryExtract("auth-service", line.AsSpan(), out var log))
{
    Console.WriteLine($"Timestamp: {log.Timestamp:o}");
    Console.WriteLine($"Level: {log.Level}");
    Console.WriteLine($"Message: {log.Message}");
}
else
{
    Console.WriteLine("Extraction failed");
}