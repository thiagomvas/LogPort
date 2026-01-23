using LogPort.Core;
using LogPort.Core.Abstractions;
using LogPort.SDK.Filters;

namespace LogPort.SDK;

public static class LogPortClientConfigExtensions
{
    /// <summary>
    /// Sets a minimum log level threshold for processing logs.
    /// Any log entry with a level lower than the specified minimum is filtered out.
    /// </summary>
    /// <param name="config">The client configuration to modify.</param>
    /// <param name="minimumLevel">
    /// The minimum log level identifier (for example, <c>"Information"</c>, <c>"Warning"</c>, <c>"Error"</c>).
    /// The comparison semantics depend on the underlying filter implementation.
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> is <c>null</c>.
    /// </exception>
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
    /// Allows only logs matching the specified log levels to be emitted.
    /// All log levels not listed are filtered out.
    /// </summary>
    /// <param name="config">The client configuration to modify.</param>
    /// <param name="allowedLevels">
    /// One or more log level identifiers to allow (for example, <c>"Information"</c>, <c>"Error"</c>).
    /// The comparison semantics depend on the underlying filter implementation.
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
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
    /// Excludes logs matching the specified log levels from being emitted.
    /// All log levels not listed are allowed.
    /// </summary>
    /// <param name="config">The client configuration to modify.</param>
    /// <param name="blacklistedLevels">
    /// One or more log level identifiers to exclude (for example, <c>"Debug"</c>, <c>"Trace"</c>).
    /// The comparison semantics depend on the underlying filter implementation.
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
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
    /// Applies sampling for a specific log level, overriding the global sampling rate if present.
    /// </summary>
    /// <param name="config">The client configuration to modify.</param>
    /// <param name="level">
    /// The log level identifier to sample (for example, <c>"Information"</c>, <c>"Warning"</c>, <c>"Error"</c>).
    /// The comparison semantics depend on the underlying filter implementation.
    /// </param>
    /// <param name="rate">
    /// The sampling rate, expressed as a value between <c>0</c> (drop all logs) and <c>1</c> (keep all logs).
    /// </param>
    /// <param name="deterministic">
    /// Indicates whether sampling decisions should be deterministic across executions
    /// (the same log inputs will yield the same sampling outcome).
    /// </param>
    /// <returns>The updated <see cref="LogPortClientConfig"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rate"/> is less than <c>0</c> or greater than <c>1</c>.
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
    /// Applies a global log sampling rate across all log levels.
    /// </summary>
    /// <remarks>
    /// This method configures probabilistic sampling to reduce log volume by allowing
    /// only a percentage of log entries to be emitted. The sampling decision is applied
    /// uniformly to all log levels unless overridden by more specific sampling rules.
    ///
    /// When <paramref name="deterministic"/> is enabled, sampling decisions are stable
    /// across executions for the same input, which is useful for consistent behavior
    /// in distributed systems and during debugging.
    /// </remarks>
    /// <param name="config">
    /// The <see cref="LogPortClientConfig"/> instance to configure.
    /// </param>
    /// <param name="rate">
    /// The sampling rate expressed as a value between <c>0</c> and <c>1</c>, where
    /// <c>0</c> disables all logs and <c>1</c> allows all logs.
    /// </param>
    /// <param name="deterministic">
    /// Indicates whether sampling should be deterministic across executions.
    /// </param>
    /// <returns>
    /// The same <see cref="LogPortClientConfig"/> instance, allowing fluent configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rate"/> is outside the inclusive range <c>0</c>–<c>1</c>.
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
    /// Disables all logging for the client by registering a filter that suppresses every log entry.
    /// </summary>
    /// <remarks>
    /// This method adds a <see cref="DisableAllLogsFilter"/> to the configuration, causing all
    /// log entries to be rejected before they are enqueued or sent to the server.
    /// <para>
    /// This is typically useful for testing scenarios, benchmarking, or environments where
    /// logging must be completely disabled at runtime.
    /// </para>
    /// </remarks>
    /// <param name="config">
    /// The <see cref="LogPortClientConfig"/> instance to configure.
    /// </param>
    /// <returns>
    /// The same <see cref="LogPortClientConfig"/> instance, allowing fluent configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> is <c>null</c>.
    /// </exception>
    public static LogPortClientConfig DisableAllLogs(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        config.Filters.Add(new DisableAllLogsFilter());
        return config;
    }


    /// <summary>
    /// Registers a log enricher to be applied to all log entries produced by the client.
    /// </summary>
    /// <remarks>
    /// Enrichers are executed for every <see cref="LogEntry"/> before it is enqueued and sent to the server.
    /// They can add or modify structured metadata, such as correlation identifiers, user context,
    /// request information, or framework-specific data (for example, Entity Framework Core details).
    /// </remarks>
    /// <param name="config">
    /// The <see cref="LogPortClientConfig"/> instance to configure.
    /// </param>
    /// <param name="enricher">
    /// The <see cref="ILogEnricher"/> to register. The enricher will be invoked in the order
    /// in which it is added.
    /// </param>
    /// <returns>
    /// The same <see cref="LogPortClientConfig"/> instance, allowing fluent configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> or <paramref name="enricher"/> is <c>null</c>.
    /// </exception>
    public static LogPortClientConfig UseEnricher(this LogPortClientConfig config, ILogEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(enricher);

        config.Enrichers ??= [];
        config.Enrichers.Add(enricher);
        return config;
    }

}