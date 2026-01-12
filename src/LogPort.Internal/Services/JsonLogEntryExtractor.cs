using System.Globalization;
using System.Text;
using System.Text.Json;

using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

namespace LogPort.Internal.Services;

public sealed class JsonLogEntryExtractor : BaseLogEntryExtractor
{
    private readonly string _messageKey;
    private readonly string _levelKey;
    private readonly string _timestampKey;

    public JsonLogEntryExtractor(JsonLogEntryExtractorConfig config)
    {
        _messageKey = config.MessageProperty;
        _levelKey = config.LevelProperty;
        _timestampKey = config.TimestampProperty;
    }
    public override bool TryExtract(ReadOnlySpan<char> input, out LogEntry result)
    {
        result = null!;

        var jsonStart = input.IndexOf('{');
        if (jsonStart < 0)
            return false;

        var jsonSpan = input[jsonStart..];

        // Unsafe code magic
        var byteCount = Encoding.UTF8.GetByteCount(jsonSpan);
        Span<byte> buffer = stackalloc byte[byteCount];

        Encoding.UTF8.GetBytes(jsonSpan, buffer);

        var reader = new Utf8JsonReader(
            buffer,
            isFinalBlock: true,
            state: default);

        var timestamp = DateTime.UtcNow;
        var level = "INFO";
        string? message = null;

        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            if (reader.ValueTextEquals(_messageKey))
            {
                reader.Read();
                message = reader.GetString();
            }
            else if (reader.ValueTextEquals(_levelKey))
            {
                reader.Read();
                level = reader.GetString()?.ToUpperInvariant() ?? level;
            }
            else if (reader.ValueTextEquals(_timestampKey))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.String &&
                    DateTime.TryParse(
                        reader.GetString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var ts))
                {
                    timestamp = ts;
                }
            }
        }

        if (message is null)
            return false;

        result = new LogEntry
        {
            Timestamp = timestamp,
            Message = message,
            Level = level
        };

        return true;
    }
}