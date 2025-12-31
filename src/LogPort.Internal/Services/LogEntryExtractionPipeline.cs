using System.Collections.Frozen;

using LogPort.Core.Models;
using LogPort.Internal.Abstractions;

namespace LogPort.Internal.Services;


public sealed class LogEntryExtractionPipeline
{
    private readonly FrozenDictionary<string, BaseLogEntryExtractor> _byService;

    public LogEntryExtractionPipeline(IEnumerable<BaseLogEntryExtractorConfig> configs)
    {
        var map = new Dictionary<string, BaseLogEntryExtractor>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            if (string.IsNullOrWhiteSpace(config.ServiceName))
                continue;

            var extractor = CreateExtractor(config);
            map[config.ServiceName] = extractor;
        }

        _byService = map.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public bool TryExtract(
        string serviceName,
        ReadOnlySpan<char> input,
        out LogEntry entry)
    {
        if (_byService.TryGetValue(serviceName, out var extractor))
        {
            return extractor.TryExtract(input, out entry);
        }

        entry = default!;
        return false;
    }

    private static BaseLogEntryExtractor CreateExtractor(
        BaseLogEntryExtractorConfig config)
    {
        return config.ExtractionMode.ToLowerInvariant() switch
        {
            "json" => new JsonLogEntryExtractor(
                (JsonLogEntryExtractorConfig)config),
            
            "regex" => new RegexLogEntryExtractor(
                (RegexLogEntryExtractorConfig)config),

            _ => throw new InvalidOperationException(
                $"Unknown extractor mode '{config.ExtractionMode}'")
        };
    }
}