using System.Text.Json.Serialization;

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

    public class PostgresConfig
    {
        /// <summary>
        /// Gets or sets whether or not it should use postgres.
        /// </summary>
        /// <remarks>
        /// Currently it is the only data store provider supported, has to be true.
        /// </remarks>
        public bool Use { get; set; } = true;
        /// <summary>
        /// Gets or sets the host of the PostgreSQL database
        /// </summary>
        public string Host { get; set; } = "localhost";
        /// <summary>
        /// Gets or sets the port of the PostgreSQL database
        /// </summary>
        public int Port { get; set; } = 5432;
        /// <summary>
        /// Gets or sets the PostgreSQL database
        /// </summary>
        public string Database { get; set; } = "logport";

        /// <summary>
        /// Gets or sets the user of the PostgreSQL database
        /// </summary>
        public string Username { get; set; } = "postgres";

        /// <summary>
        /// Gets or sets the password of the PostgreSQL database
        /// </summary>
        public string Password { get; set; } = "postgres";

        [JsonIgnore]
        public string ConnectionString =>
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";

        /// <summary>
        /// Gets or sets the partition length in days of the logs table
        /// </summary>
        public int PartitionLength { get; set; } = 1;
    }

    public class DockerConfig
    {
        /// <summary>
        /// Gets or sets whether to enable the Docker module.
        /// </summary>
        public bool Use { get; set; } = false;
        /// <summary>
        /// Gets or sets the Docker socket path.
        /// </summary>
        public string SocketPath { get; set; } = "unix:///var/run/docker.sock";

        /// <summary>
        /// Gets or sets whether the agent should monitor <b>EVERY</b> container in the host.
        /// </summary>
        /// <remarks>
        /// Depending on the environment, this can lead to excessive log throughput.
        /// It is recommended to use labels to mark which containers should be monitored
        /// </remarks>
        public bool WatchAllContainers { get; set; } = false;
    }

    public class CacheConfig
    {
        /// <summary>
        /// Gets or sets whether the cache should use redis or not.
        /// </summary>
        public bool UseRedis { get; set; } = false;
        /// <summary>
        /// Gets or sets the connection string for the redis cache.
        /// </summary>
        public string? RedisConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the default expiration of cached data.
        /// </summary>
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(10);


    }

}