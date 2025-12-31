using System.Globalization;
using System.Text.RegularExpressions;
using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

namespace LogPort.Internal.Services;

public sealed class RegexLogEntryExtractor : BaseLogEntryExtractor
{
    private readonly Regex _regex;
    private readonly string _messageGroup;
    private readonly string _levelGroup;
    private readonly string _timestampGroup;

    public RegexLogEntryExtractor(RegexLogEntryExtractorConfig config)
    {
        _regex = new Regex(
            config.Pattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        _messageGroup = config.MessageGroup;
        _levelGroup = config.LevelGroup;
        _timestampGroup = config.TimestampGroup;
    }

    public override bool TryExtract(ReadOnlySpan<char> input, out LogEntry result)
    {
        result = default;

        var match = _regex.Match(input.ToString());
        if (!match.Success)
            return false;

        var messageGroup = match.Groups[_messageGroup];
        if (!messageGroup.Success)
            return false;

        var level = "INFO";
        var timestamp = DateTime.UtcNow;

        var levelGroup = match.Groups[_levelGroup];
        if (levelGroup.Success)
            level = levelGroup.Value.ToUpperInvariant();

        var tsGroup = match.Groups[_timestampGroup];
        if (tsGroup.Success &&
            DateTime.TryParse(
                tsGroup.Value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var ts))
        {
            timestamp = ts;
        }

        result = new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Message = messageGroup.Value
        };

        return true;
    }
}