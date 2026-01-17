using LogPort.SDK.Filters;

namespace LogPort.SDK;

public sealed class LogPortClientConfig
{
    /// <summary>
    /// Gets or sets the base URL of the LogPort agent.
    /// </summary>
    public string AgentUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logical service name associated with emitted logs.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the deployment environment name (e.g. Production, Staging).
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the hostname associated with emitted logs.
    /// </summary>
    public string? Hostname { get; set; }

    /// <summary>
    /// Gets or sets the API secret used to authenticate with the LogPort agent.
    /// </summary>
    public string ApiSecret { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of log level filters applied before sending logs.
    /// </summary>
    public List<ILogLevelFilter>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the maximum delay between reconnection attempts.
    /// </summary>
    public TimeSpan ClientMaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the interval at which heartbeat messages are sent.
    /// </summary>
    public TimeSpan ClientHeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the timeout for heartbeat responses before the connection is considered dead.
    /// </summary>
    public TimeSpan ClientHeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public static LogPortClientConfig LoadFromEnvironment()
    {
        var config = new LogPortClientConfig();

        var agentUrl = System.Environment.GetEnvironmentVariable("LOGPORT_SERVER_URL");

        if (!string.IsNullOrWhiteSpace(agentUrl))
        {
            config.AgentUrl = agentUrl;
        }

        config.ApiSecret = System.Environment.GetEnvironmentVariable("LOGPORT_SECRET") ?? string.Empty;

        config.ServiceName = System.Environment.GetEnvironmentVariable("LOGPORT_SERVICE_NAME");

        var maxReconnectDelayStr = System.Environment.GetEnvironmentVariable("LOGPORT_CLIENT_MAX_RECONNECT_DELAY");
        if (TimeSpan.TryParse(maxReconnectDelayStr, out var maxReconnectDelay))
        {
            config.ClientMaxReconnectDelay = maxReconnectDelay;
        }

        var heartbeatIntervalStr = System.Environment.GetEnvironmentVariable("LOGPORT_CLIENT_HEARTBEAT_INTERVAL");
        if (TimeSpan.TryParse(heartbeatIntervalStr, out var heartbeatInterval))
        {
            config.ClientHeartbeatInterval = heartbeatInterval;
        }

        var heartbeatTimeoutStr = System.Environment.GetEnvironmentVariable("LOGPORT_CLIENT_HEARTBEAT_TIMEOUT");
        if (TimeSpan.TryParse(heartbeatTimeoutStr, out var heartbeatTimeout))
        {
            config.ClientHeartbeatTimeout = heartbeatTimeout;
        }

        var environment = System.Environment.GetEnvironmentVariable("LOGPORT_ENVIRONMENT");
        if (!string.IsNullOrWhiteSpace(environment))
        {
            config.Environment = environment;
        }
        var hostname = System.Environment.GetEnvironmentVariable("LOGPORT_HOSTNAME");
        if (!string.IsNullOrWhiteSpace(hostname))
        {
            config.Hostname = hostname;
        }

        return config;
    }
}