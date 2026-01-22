namespace LogPort.Internal.Configuration;

public class LogPortConfig
{
    /// <summary>
    /// Gets or sets the configuration for the PostgreSQL module in the agent.
    /// </summary>
    public PostgresConfig Postgres { get; set; } = new();
    /// <summary>
    /// Gets or sets the configuration for the Docker module in the agent.
    /// </summary>
    public DockerConfig Docker { get; set; } = new();
    /// <summary>
    /// Gets or sets the configuration for the caching module in the agent.
    /// </summary>
    public CacheConfig Cache { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for the metrics module in the agent.
    /// </summary>
    public MetricsConfig Metrics { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the configuration for the retention module and job in the agent.
    /// </summary>
    public LogRetentionConfig Retention { get; set; } = new();

    /// <summary>
    /// Gets or sets the port that the agent will listen on.
    /// </summary>
    public uint Port { get; set; } = 8080;
    /// <summary>
    /// Gets or sets the Upstream Agent URL.
    /// </summary>
    /// <remarks>
    /// This is only used when <see cref="LogPortConfig.Mode"/> is <see cref="LogMode.Relay"/>.
    /// It represents the URL where the relay will forward all request and data received.
    /// </remarks>
    public string? UpstreamUrl { get; set; }

    /// <summary>
    /// Gets or sets the size of the batch that logs will be processed in the background.
    /// </summary>
    public int BatchSize { get; set; } = 100;
    /// <summary>
    /// Gets or sets the interval that log batches will be processed in milliseconds.
    /// </summary>
    public int FlushIntervalMs { get; set; } = 250;

    /// <summary>
    /// Gets or sets the administrator username used for HTTP Basic authentication.
    /// </summary>
    public string AdminLogin { get; set; } = "admin";

    /// <summary>
    /// Gets or sets the administrator password used for HTTP Basic authentication.
    /// </summary>
    /// <remarks>
    /// This value should be provided via configuration or environment variables
    /// and should not be hard-coded in production environments.
    /// </remarks>
    public string AdminPassword { get; set; } = "changeme";

    /// <summary>
    /// Gets or sets the shared secret used for API token authentication.
    /// </summary>
    /// <remarks>
    /// This value is used to authenticate applications to the agent, allowing them
    /// to stream logs directly to LogPort.
    /// </remarks>
    public string ApiSecret { get; set; } = Guid.NewGuid().ToString("N");


    /// <summary>
    /// Gets or sets log extraction rules used by modules that can only extract context from messages (e.g. Docker).
    /// </summary>
    public List<BaseLogEntryExtractorConfig> Extractors { get; set; } = new();

    /// <summary>
    /// Gets or sets a list of files that are tailed for log messages.
    /// </summary>
    public List<FileTailingConfiguration> FileTails { get; set; } = new();

    /// <summary>
    /// Gets or sets the mode of the agent.
    /// </summary>
    public LogMode Mode { get; set; } = LogMode.Agent;

    public string JwtSecret { get; set; } = "";
    public string JwtIssuer { get; set; } = "";

    private static bool GetEnvBool(string key, bool defaultValue = false)
    {
        return bool.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
    }

    private static int GetEnvInt(string key, int defaultValue)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
    }

    private static uint GetEnvUInt(string key, uint defaultValue)
    {
        return uint.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
    }
}