namespace LogPort.Internal;

public static class EnvVars
{
    // Core
    public const string ConfigPath = "LOGPORT_CONFIG_PATH";
    public const string Port = "LOGPORT_PORT";
    public const string UpstreamUrl = "LOGPORT_UPSTREAM_URL";
    public const string BatchSize = "LOGPORT_BATCH_SIZE";
    public const string FlushIntervalMs = "LOGPORT_FLUSH_INTERVAL_MS";
    public const string Mode = "LOGPORT_MODE";
    public const string AdminLogin = "LOGPORT_ADMIN_LOGIN";
    public const string AdminPassword = "LOGPORT_ADMIN_PASS";

    // Postgres
    public const string UsePostgres = "LOGPORT_USE_POSTGRES";
    public const string PostgresHost = "LOGPORT_POSTGRES_HOST";
    public const string PostgresPort = "LOGPORT_POSTGRES_PORT";
    public const string PostgresDatabase = "LOGPORT_POSTGRES_DATABASE";
    public const string PostgresUsername = "LOGPORT_POSTGRES_USERNAME";
    public const string PostgresPassword = "LOGPORT_POSTGRES_PASSWORD";
    public const string PostgresPartitionLength = "LOGPORT_POSTGRES_PARTITION_LENGTH";

    // Docker
    public const string UseDocker = "LOGPORT_USE_DOCKER";
    public const string DockerSocketPath = "LOGPORT_DOCKER_SOCKET_PATH";
    public const string DockerExtractorConfigPath = "LOGPORT_DOCKER_EXTRACTOR_CONFIG_PATH";
    public const string DockerWatchAll = "LOGPORT_DOCKER_WATCH_ALL";

    // Cache
    public const string UseRedis = "LOGPORT_CACHE_USE_REDIS";
    public const string RedisConnectionString = "LOGPORT_CACHE_REDIS_CONNECTION_STRING";
    public const string CacheDefaultExpirationMs = "LOGPORT_CACHE_DEFAULT_EXPIRATION_MS";
    
}