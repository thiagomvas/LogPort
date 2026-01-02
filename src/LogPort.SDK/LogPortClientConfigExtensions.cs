using LogPort.Core;

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

    public static LogPortClientConfig DisableAllLogs(this LogPortClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.Filters ??= [];
        
        config.Filters.Add(new DisableAllLogsFilter());
        return config;
    }
}