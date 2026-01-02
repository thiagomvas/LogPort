using LogPort.Core;
using LogPort.SDK.Filters;

namespace LogPort.SDK;

public static class LogPortClientConfigExtensions
{
    /// <summary>
    /// Sets the minimum log level that will be processed.
    /// Logs below this level are filtered out.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="minimumLevel">The minimum log level name.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig UseMimimumLevel(
        this LogPortClientConfig config,
        string minimumLevel)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        config.Filters.Add(new MinimumLogLevelFilter(minimumLevel));
        return config;
    }

    /// <summary>
    /// Allows only the specified log levels.
    /// All other levels are filtered out.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="allowedLevels">Log levels that are allowed.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig UseLevelWhitelist(
        this LogPortClientConfig config,
        params string[] allowedLevels)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        config.Filters.Add(new WhitelistLogLevelFilter(allowedLevels));
        return config;
    }

    /// <summary>
    /// Filters out the specified log levels.
    /// All other levels are allowed.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="blacklistedLevels">Log levels to exclude.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig UseLevelBlacklist(
        this LogPortClientConfig config,
        params string[] blacklistedLevels)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        config.Filters.Add(new BlacklistLogLevelFilter(blacklistedLevels));
        return config;
    }

    /// <summary>
    /// Applies sampling for a specific log level.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="level">The log level to sample.</param>
    /// <param name="rate">Sampling rate between 0 and 1.</param>
    /// <param name="deterministic">
    /// Whether sampling should be deterministic across executions.
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rate"/> is outside the range 0–1.
    /// </exception>
    public static LogPortClientConfig UseSampling(
        this LogPortClientConfig config,
        string level,
        double rate,
        bool deterministic = true)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (rate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(rate));

        config.Filters ??= [];
        var sampler = config.Filters
            .OfType<SamplingLogLevelFilter>()
            .FirstOrDefault();

        if (sampler == null)
        {
            sampler = new SamplingLogLevelFilter(deterministic);
            config.Filters.Add(sampler);
        }

        sampler.SetRate(level, rate);
        return config;
    }

    /// <summary>
    /// Applies a global sampling rate for all log levels.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <param name="rate">Sampling rate between 0 and 1.</param>
    /// <param name="deterministic">
    /// Whether sampling should be deterministic across executions.
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rate"/> is outside the range 0–1.
    /// </exception>
    public static LogPortClientConfig UseGlobalSampling(
        this LogPortClientConfig config,
        double rate,
        bool deterministic = true)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (rate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(rate));

        config.Filters ??= [];

        var sampler = config.Filters
            .OfType<SamplingLogLevelFilter>()
            .FirstOrDefault();

        if (sampler == null)
        {
            sampler = new SamplingLogLevelFilter(deterministic);
            config.Filters.Add(sampler);
        }

        sampler.SetDefaultRate(rate);
        return config;
    }

    /// <summary>
    /// Disables all logging.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <returns>The updated <see cref="LogPortClientConfig"/>.</returns>
    public static LogPortClientConfig DisableAllLogs(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        config.Filters.Add(new DisableAllLogsFilter());
        return config;
    }
}