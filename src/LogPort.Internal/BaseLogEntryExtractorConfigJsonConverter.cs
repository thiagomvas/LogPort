using System.Text.Json;
using System.Text.Json.Serialization;

using LogPort.Internal.Configuration;

namespace LogPort.Internal;

public sealed class BaseLogEntryExtractorConfigJsonConverter
    : JsonConverter<BaseLogEntryExtractorConfig>
{
    public override BaseLogEntryExtractorConfig Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("ExtractionMode", out var modeProp))
            throw new JsonException("Extractor config must define ExtractionMode");

        var mode = modeProp.GetString();

        return (mode?.ToLowerInvariant() switch
        {
            "json" => root.Deserialize<JsonLogEntryExtractorConfig>(options)!,
            "regex" => root.Deserialize<RegexLogEntryExtractorConfig>(options)!,

            _ => root.Deserialize<BaseLogEntryExtractorConfig>(options)
        })!;
    }

    public override void Write(
        Utf8JsonWriter writer,
        BaseLogEntryExtractorConfig value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}