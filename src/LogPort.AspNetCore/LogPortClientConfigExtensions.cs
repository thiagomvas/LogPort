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

    /// <summary>
    /// Sets the minimum log level using ASP.NET Core <see cref="LogLevel"/> values.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="minimumLevel">The minimum ASP.NET Core log level.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    /// <remarks>
    /// When <see cref="LogLevel.None"/> is specified, all logging is disabled.
    /// </remarks>
    public static LogPortClientConfig UseMimimumLevel(
        this LogPortClientConfig config,
        LogLevel minimumLevel)
    {
        if (minimumLevel == LogLevel.None) return config.DisableAllLogs();
        return config.UseMimimumLevel(AspNetToLogPortMap[minimumLevel]);
    }

    /// <summary>
    /// Allows only the specified ASP.NET Core log levels.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="allowedLevels">The ASP.NET Core log levels to allow.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig UseLevelWhitelist(
        this LogPortClientConfig config,
        params LogLevel[] allowedLevels)
    {
        return config.UseLevelWhitelist(
            allowedLevels.Select(l => AspNetToLogPortMap[l]).ToArray());
    }

    /// <summary>
    /// Excludes the specified ASP.NET Core log levels.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="blacklistedLevels">The ASP.NET Core log levels to exclude.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig UseLevelBlacklist(
        this LogPortClientConfig config,
        params LogLevel[] blacklistedLevels)
    {
        return config.UseLevelBlacklist(
            blacklistedLevels.Select(l => AspNetToLogPortMap[l]).ToArray());
    }

    /// <summary>
    /// Applies sampling to a specific ASP.NET Core log level.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="level">The ASP.NET Core log level to sample.</param>
    /// <param name="rate">Sampling rate between 0 and 1.</param>
    /// <param name="deterministic">
    /// Whether sampling should be deterministic across executions.
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig UseSampling(
        this LogPortClientConfig config,
        LogLevel level,
        double rate,
        bool deterministic = true)
    {
        return config.UseSampling(AspNetToLogPortMap[level], rate, deterministic);
    }
}