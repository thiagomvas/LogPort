using LogPort.Core;

namespace LogPort.SDK;

internal static class LogLevelSeverity
{
    public static readonly IReadOnlyDictionary<string, int> Map =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [LogNormalizer.TraceLevel]   = 0,
            [LogNormalizer.DebugLevel]   = 1,
            [LogNormalizer.InfoLevel]    = 2,
            [LogNormalizer.WarningLevel] = 3,
            [LogNormalizer.ErrorLevel]   = 4,
            [LogNormalizer.FatalLevel]   = 5,
        };
}