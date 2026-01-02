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
    
}