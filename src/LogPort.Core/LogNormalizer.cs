using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogPort.Core;

public sealed partial class LogNormalizer
{
    public const string DefaultLevel = "Info";
    public const string InfoLevel = "Info";
    public const string WarningLevel = "Warn";
    public const string ErrorLevel = "Error";
    public const string FatalLevel = "Fatal";
    public const string DebugLevel = "Debug";
    public const string TraceLevel = "Trace";
    
    private readonly ConcurrentDictionary<string, string> _levelMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        ["trace"] = TraceLevel,
        ["debug"] = DebugLevel,
        ["info"] = InfoLevel,
        ["information"] = InfoLevel,
        ["warn"] = WarningLevel,
        ["warning"] = WarningLevel,
        ["error"] = ErrorLevel,
        ["err"] = ErrorLevel,
        ["fail"] = ErrorLevel,
        ["failure"] = ErrorLevel,
        ["critical"] = FatalLevel,
        ["fatal"] = FatalLevel,
        ["panic"] = FatalLevel
    };

    public string NormalizeLevel(string level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return DefaultLevel;

        return _levelMapping.GetOrAdd(level, static key =>
        {
            if (InfoRegex().IsMatch(key))
                return InfoLevel;
            if (WarnRegex().IsMatch(key))
                return WarningLevel;
            if (ErrorRegex().IsMatch(key))
                return ErrorLevel;
            if (CriticalRegex().IsMatch(key))
                return FatalLevel;
            if (DebugRegex().IsMatch(key))
                return DebugLevel;
            if (TraceRegex().IsMatch(key))
                return TraceLevel;

            return DefaultLevel;
        });
    }
    [GeneratedRegex(@"in(?:fo(?:rm(?:ation|ational)?)?|f)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex InfoRegex();

    [GeneratedRegex(@"warn(?:ing|g)?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex WarnRegex();

    [GeneratedRegex(@"err(?:or)?|fail(?:ed|ure)?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ErrorRegex();

    [GeneratedRegex(@"crit(?:ical)?|fatal|panic", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CriticalRegex();

    [GeneratedRegex(@"deb(?:ug|g)?|dbg", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DebugRegex();

    [GeneratedRegex(@"tr(?:ace|c|ce)?|verb(?:ose)?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex TraceRegex();

}
