using LogPort.Core;
using LogPort.SDK.Filters;

namespace LogPort.SDK;

public static class LogPortClientConfigExtensions
{
    public static LogPortClientConfig UseMimimumLevel(
        this LogPortClientConfig config,
        string minimumLevel)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        
        config.Filters.Add(new MinimumLogLevelFilter(minimumLevel));
        return config;
    }

    public static LogPortClientConfig UseLevelWhitelist(
        this LogPortClientConfig config,
        params string[] allowedLevels)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        
        config.Filters.Add(new WhitelistLogLevelFilter(allowedLevels));
        return config;
    }
    
    public static LogPortClientConfig UseLevelBlacklist(
        this LogPortClientConfig config,
        params string[] blacklistedLevels)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        
        config.Filters.Add(new BlacklistLogLevelFilter(blacklistedLevels));
        return config;
    }
    
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

    

    public static LogPortClientConfig DisableAllLogs(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        
        config.Filters.Add(new DisableAllLogsFilter());
        return config;
    }
}