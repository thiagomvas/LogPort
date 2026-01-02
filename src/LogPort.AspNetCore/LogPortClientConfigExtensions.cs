using LogPort.Core;
using LogPort.SDK;

using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

public static class LogPortClientConfigExtensions
{
    private static readonly IReadOnlyDictionary<LogLevel, string> AspNetToLogPortMap =
        new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, LogNormalizer.TraceLevel },
            { LogLevel.Debug, LogNormalizer.DebugLevel },
            { LogLevel.Information, LogNormalizer.InfoLevel },
            { LogLevel.Warning, LogNormalizer.WarningLevel },
            { LogLevel.Error, LogNormalizer.ErrorLevel },
            { LogLevel.Critical, LogNormalizer.FatalLevel }
        };

    public static LogPortClientConfig UseMimimumLevel(
        this LogPortClientConfig config,
        LogLevel minimumLevel)
    {
        if (minimumLevel == LogLevel.None) return config.DisableAllLogs();
        return config.UseMimimumLevel(AspNetToLogPortMap[minimumLevel]);
    }

    public static LogPortClientConfig UseLevelWhitelist(
        this LogPortClientConfig config,
        params LogLevel[] allowedLevels)
    {
        return config.UseLevelWhitelist(allowedLevels.Select(l => l.ToString()).ToArray());
    }
    
    public static LogPortClientConfig UseLevelBlacklist(
        this LogPortClientConfig config,
        params LogLevel[] blacklistedLevels)
    {
        return config.UseLevelBlacklist(blacklistedLevels.Select(l => l.ToString()).ToArray());
    }
}