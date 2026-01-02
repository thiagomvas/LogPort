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

    public static LogPortClientConfig UseMinimumLevel(
        this LogPortClientConfig config,
        LogLevel minimumLevel)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];

        config.Filters.Add(
            new MinimumLogLevelFilter(AspNetToLogPortMap[minimumLevel])
        );

        return config;
    }

    public static LogPortClientConfig UseLevelWhitelist(
        this LogPortClientConfig config,
        params LogLevel[] allowedLevels)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];

        var mappedLevels = allowedLevels
            .Select(level => AspNetToLogPortMap[level])
            .ToArray();

        config.Filters.Add(
            new WhitelistLogLevelFilter(mappedLevels)
        );

        return config;
    }
}