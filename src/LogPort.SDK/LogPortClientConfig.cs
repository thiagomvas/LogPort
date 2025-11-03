namespace LogPort.SDK;

public sealed class LogPortClientConfig
{
    public string AgentUrl { get; set; }
    public string? ServiceName { get; set; }
    public TimeSpan ClientMaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan ClientHeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan ClientHeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    public static LogPortClientConfig LoadFromEnvironment()
    {
        var config = new LogPortClientConfig();

        var agentUrl = Environment.GetEnvironmentVariable("LOGPORT_SERVER_URL");
        
        if (!string.IsNullOrWhiteSpace(agentUrl))
        {
            config.AgentUrl = agentUrl;
        }

        config.ServiceName = Environment.GetEnvironmentVariable("LOGPORT_SERVICE_NAME");

        var maxReconnectDelayStr = Environment.GetEnvironmentVariable("LOGPORT_CLIENT_MAX_RECONNECT_DELAY");
        if (TimeSpan.TryParse(maxReconnectDelayStr, out var maxReconnectDelay))
        {
            config.ClientMaxReconnectDelay = maxReconnectDelay;
        }

        var heartbeatIntervalStr = Environment.GetEnvironmentVariable("LOGPORT_CLIENT_HEARTBEAT_INTERVAL");
        if (TimeSpan.TryParse(heartbeatIntervalStr, out var heartbeatInterval))
        {
            config.ClientHeartbeatInterval = heartbeatInterval;
        }

        var heartbeatTimeoutStr = Environment.GetEnvironmentVariable("LOGPORT_CLIENT_HEARTBEAT_TIMEOUT");
        if (TimeSpan.TryParse(heartbeatTimeoutStr, out var heartbeatTimeout))
        {
            config.ClientHeartbeatTimeout = heartbeatTimeout;
        }

        return config;
    }
}