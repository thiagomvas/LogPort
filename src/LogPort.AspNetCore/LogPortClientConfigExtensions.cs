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

    /// <summary>
    /// Registers the default ASP.NET Core HTTP enrichers.
    /// </summary>
    /// <remarks>
    /// This is a convenience method that adds both <see cref="HttpRequestEnricher"/> and
    /// <see cref="HttpResponseEnricher"/> to the client configuration.
    /// These enrichers populate HTTP-related metadata such as request method, path,
    /// host, and response status code when an active HTTP context is present.
    /// </remarks>
    /// <param name="config">The LogPort client configuration to modify.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> is <c>null</c>.
    /// </exception>
    public static LogPortClientConfig UseHttpEnrichers(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Enrichers ??= [];
        config.Enrichers.Add(new HttpRequestEnricher());
        config.Enrichers.Add(new HttpResponseEnricher());
        return config;
    }

    /// <summary>
    /// Registers an enricher that adds HTTP request metadata to log entries.
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpRequestEnricher"/> enriches log entries with information from
    /// the current ASP.NET Core <c>HttpContext</c>, such as HTTP method, scheme, host,
    /// and request path. If no active HTTP context exists, this enricher is a no-op.
    /// </remarks>
    /// <param name="config">The LogPort client configuration to modify.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> is <c>null</c>.
    /// </exception>
    public static LogPortClientConfig UseHttpRequestEnricher(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Enrichers ??= [];
        config.Enrichers.Add(new HttpRequestEnricher());
        return config;
    }

    /// <summary>
    /// Registers an enricher that adds HTTP response metadata to log entries.
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpResponseEnricher"/> enriches log entries with response-related
    /// information from the current ASP.NET Core <c>HttpContext</c>, such as the HTTP
    /// status code. If no active HTTP context exists, this enricher is a no-op.
    /// </remarks>
    /// <param name="config">The LogPort client configuration to modify.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> is <c>null</c>.
    /// </exception>
    public static LogPortClientConfig UseHttpResponseEnricher(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Enrichers ??= [];
        config.Enrichers.Add(new HttpResponseEnricher());
        return config;
    }
}