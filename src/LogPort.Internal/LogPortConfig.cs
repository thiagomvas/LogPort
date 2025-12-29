namespace LogPort.Internal;

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
    /// Gets or sets the mode of the agent.
    /// </summary>
    public LogMode Mode { get; set; } = LogMode.Agent;

    public static LogPortConfig LoadFromEnvironment()
    {
        var config = new LogPortConfig();

        config.Port = GetEnvUInt("LOGPORT_PORT", 8080);
        config.UpstreamUrl = Environment.GetEnvironmentVariable("LOGPORT_UPSTREAM_URL")?.Trim('/');
        config.BatchSize = GetEnvInt("LOGPORT_BATCH_SIZE", 100);
        config.FlushIntervalMs = GetEnvInt("LOGPORT_FLUSH_INTERVAL_MS", 250);
        var modeStr = Environment.GetEnvironmentVariable("LOGPORT_MODE") ?? "Agent";
        config.Mode = Enum.TryParse<LogMode>(modeStr, true, out var mode) ? mode : LogMode.Agent;


        // Postgres
        config.Postgres.Use = GetEnvBool("LOGPORT_USE_POSTGRES");
        config.Postgres.Host = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_HOST") ?? "localhost";
        config.Postgres.Port = GetEnvInt("LOGPORT_POSTGRES_PORT", 5432);
        config.Postgres.Database = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_DATABASE") ?? "logport";
        config.Postgres.Username = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_USERNAME") ?? "postgres";
        config.Postgres.Password = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_PASSWORD") ?? "postgres";
        config.Postgres.PartitionLength = GetEnvInt("LOGPORT_POSTGRES_PARTITION_LENGTH", 1);

        // Docker
        config.Docker.Use = GetEnvBool("LOGPORT_USE_DOCKER");
        config.Docker.SocketPath = Environment.GetEnvironmentVariable("LOGPORT_DOCKER_SOCKET_PATH") ?? "unix:///var/run/docker.sock";
        config.Docker.ExtractorConfigPath = Environment.GetEnvironmentVariable("LOGPORT_DOCKER_EXTRACTOR_CONFIG_PATH");
        config.Docker.WatchAllContainers = GetEnvBool("LOGPORT_DOCKER_WATCH_ALL");

        // Cache
        config.Cache.UseRedis = GetEnvBool("LOGPORT_CACHE_USE_REDIS");
        config.Cache.RedisConnectionString = Environment.GetEnvironmentVariable("LOGPORT_CACHE_REDIS_CONNECTION_STRING");
        config.Cache.DefaultExpiration =
            TimeSpan.FromMilliseconds(GetEnvInt("LOGPORT_CACHE_DEFAULT_EXPIRATION_MS", 600000));
        if (config.Mode is LogMode.Agent && !config.Postgres.Use)
            throw new InvalidOperationException("At least one storage backend must be enabled.");
        return config;
    }

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
        /// Gets or sets the path of a file containing Docker log extraction rules for enriching logs processed by the Agent.
        /// </summary>
        public string? ExtractorConfigPath { get; set; }
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