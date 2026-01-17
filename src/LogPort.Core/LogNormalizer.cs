using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace LogPort.Core;

/// <summary>
/// Provides functionality for normalizing log messages and log levels, enabling grouping, pattern detection, and aggregation.
/// </summary>
public sealed partial class LogNormalizer
{
    private static readonly SHA256 _sha256 = SHA256.Create();

    public const string DefaultLevel = "Info";
    public const string InfoLevel = "Info";
    public const string WarningLevel = "Warn";
    public const string ErrorLevel = "Error";
    public const string FatalLevel = "Fatal";
    public const string DebugLevel = "Debug";
    public const string TraceLevel = "Trace";
    public const string NoneLevel = "None";
    
    /// <summary>
    /// Maps various textual representations of log levels to normalized levels.
    /// The mapping is case-insensitive and cached for performance.
    /// </summary>
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

    /// <summary>
    /// Normalizes a log message by replacing variable values with placeholders
    /// and removing commonly changing elements such as timestamps, GUIDs,
    /// file paths, and numbers.
    /// </summary>
    /// <param name="message">The original log message.</param>
    /// <param name="metadata">
    /// Optional metadata whose values will be replaced by named placeholders.
    /// </param>
    /// <returns>The normalized log message.</returns>
    public string NormalizeMessage(string message, Dictionary<string, object>? metadata = null)
    {
        string result = message ?? string.Empty;
        foreach (var kvp in metadata ?? [])
        {
            var valueString = kvp.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(valueString))
                continue;
            result = result.Replace(valueString, $"{{{kvp.Key}}}", StringComparison.OrdinalIgnoreCase);
        }

        result = IsoTimestampRegex().Replace(result, "{timestamp}");
        result = GuidRegex().Replace(result, "{guid}");
        result = UnixPathRegex().Replace(result, "{path}");
        result = NumberRegex().Replace(result, "{number}");

        return result;
    }

    
    /// <summary>
    /// Computes a deterministic hash for a normalized log pattern.
    /// Used for fast comparisons and grouping.
    /// </summary>
    /// <param name="text">The normalized message text.</param>
    /// <returns>An unsigned 64-bit hash value.</returns>
    public static ulong ComputePatternHash(string text)
    {
        const ulong offset = 14695981039346656037;
        const ulong prime = 1099511628211;

        ulong hash = offset;
        foreach (var c in text)
        {
            hash ^= c;
            hash *= prime;
        }

        return hash;
    }

    /// <summary>
    /// Normalizes a raw log level string into a known canonical level.
    /// </summary>
    /// <param name="level">The raw log level value.</param>
    /// <returns>A normalized log level.</returns>
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

    [GeneratedRegex(@"\b\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:[.,]\d+)?(?:Z|[+-]\d{2}:\d{2})?\b",
        RegexOptions.Compiled)]
    private static partial Regex IsoTimestampRegex();

    [GeneratedRegex(@"\b[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[1-5][a-fA-F0-9]{3}-[89abAB][a-fA-F0-9]{3}-[a-fA-F0-9]{12}\b",
        RegexOptions.Compiled)]
    private static partial Regex GuidRegex();

    [GeneratedRegex(@"(?:\/[^\/\s]+)+",
        RegexOptions.Compiled)]
    private static partial Regex UnixPathRegex();

    [GeneratedRegex(@"\b-?\d+(?:\.\d+)?\b",
        RegexOptions.Compiled)]
    private static partial Regex NumberRegex();


}