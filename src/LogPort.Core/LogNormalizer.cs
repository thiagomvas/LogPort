using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogPort.Core;

public sealed partial class LogNormalizer
{
    private readonly ConcurrentDictionary<string, string> _levelMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        ["trace"] = "Trace",
        ["debug"] = "Debug",
        ["info"] = "Info",
        ["information"] = "Info",
        ["warn"] = "Warn",
        ["warning"] = "Warn",
        ["error"] = "Error",
        ["fatal"] = "Fatal",
        ["critical"] = "Fatal"
    };

    public string NormalizeLevel(string level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return "Info";

        return _levelMapping.GetOrAdd(level, static key =>
        {
            if (InfoRegex().IsMatch(key))
                return "Info";
            if (WarnRegex().IsMatch(key))
                return "Warn";
            if (ErrorRegex().IsMatch(key))
                return "Error";
            if (CriticalRegex().IsMatch(key))
                return "Fatal";
            if (DebugRegex().IsMatch(key))
                return "Debug";
            if (TraceRegex().IsMatch(key))
                return "Trace";

            return "Info";
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
