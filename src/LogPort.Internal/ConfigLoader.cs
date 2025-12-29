using System.Text.Json;

namespace LogPort.Internal;

public static class ConfigLoader
{
    public static LogPortConfig Load()
    {
        bool fileExists = File.Exists("config.json");
        LogPortConfig? result = null;

        if (fileExists)
        {
            var config = File.ReadAllText("config.json");
            result = JsonSerializer.Deserialize<LogPortConfig>(config);
        }

        result ??= new LogPortConfig();

        if (!fileExists)
        {
            File.WriteAllText("config.json", JsonSerializer.Serialize(result));
        }

        LoadFromEnvironment(result);

        return result;
    }

    /// <summary>
    /// Loads the LogPort configuration from environment values, overriding an existing instance of the config class.
    /// </summary>
    /// <param name="target">The target config instance to override values.</param>
    /// <returns>The same <see cref="LogPortConfig"/> instance with environment overrides applied.</returns>
    public static LogPortConfig LoadFromEnvironment(LogPortConfig target)
    {
        // Core settings
        target.Port = GetEnvUInt("LOGPORT_PORT", target.Port);
        target.UpstreamUrl = GetEnvString("LOGPORT_UPSTREAM_URL", target.UpstreamUrl)?.Trim('/');
        target.BatchSize = GetEnvInt("LOGPORT_BATCH_SIZE", target.BatchSize);
        target.FlushIntervalMs = GetEnvInt("LOGPORT_FLUSH_INTERVAL_MS", target.FlushIntervalMs);

        var modeStr = GetEnvString("LOGPORT_MODE", target.Mode.ToString());
        if (Enum.TryParse<LogMode>(modeStr, true, out var mode))
            target.Mode = mode;

        target.ClientMaxReconnectDelay = TimeSpan.FromMilliseconds(
            GetEnvInt("LOGPORT_CLIENT_MAX_RECONNECT_DELAY_MS", (int)target.ClientMaxReconnectDelay.TotalMilliseconds));
        target.ClientHeartbeatInterval = TimeSpan.FromMilliseconds(
            GetEnvInt("LOGPORT_CLIENT_HEARTBEAT_INTERVAL_MS", (int)target.ClientHeartbeatInterval.TotalMilliseconds));
        target.ClientHeartbeatTimeout = TimeSpan.FromMilliseconds(
            GetEnvInt("LOGPORT_CLIENT_HEARTBEAT_TIMEOUT_MS", (int)target.ClientHeartbeatTimeout.TotalMilliseconds));

        // Postgres
        target.Postgres.Use = GetEnvBool("LOGPORT_USE_POSTGRES", target.Postgres.Use);
        target.Postgres.Host = GetEnvString("LOGPORT_POSTGRES_HOST", target.Postgres.Host);
        target.Postgres.Port = GetEnvInt("LOGPORT_POSTGRES_PORT", target.Postgres.Port);
        target.Postgres.Database = GetEnvString("LOGPORT_POSTGRES_DATABASE", target.Postgres.Database);
        target.Postgres.Username = GetEnvString("LOGPORT_POSTGRES_USERNAME", target.Postgres.Username);
        target.Postgres.Password = GetEnvString("LOGPORT_POSTGRES_PASSWORD", target.Postgres.Password);
        target.Postgres.PartitionLength =
            GetEnvInt("LOGPORT_POSTGRES_PARTITION_LENGTH", target.Postgres.PartitionLength);

        // Docker
        target.Docker.Use = GetEnvBool("LOGPORT_USE_DOCKER", target.Docker.Use);
        target.Docker.SocketPath = GetEnvString("LOGPORT_DOCKER_SOCKET_PATH", target.Docker.SocketPath);
        target.Docker.ExtractorConfigPath =
            GetEnvString("LOGPORT_DOCKER_EXTRACTOR_CONFIG_PATH", target.Docker.ExtractorConfigPath);
        target.Docker.WatchAllContainers = GetEnvBool("LOGPORT_DOCKER_WATCH_ALL", target.Docker.WatchAllContainers);

        // Cache
        target.Cache.UseRedis = GetEnvBool("LOGPORT_CACHE_USE_REDIS", target.Cache.UseRedis);
        target.Cache.RedisConnectionString =
            GetEnvString("LOGPORT_CACHE_REDIS_CONNECTION_STRING", target.Cache.RedisConnectionString);
        target.Cache.DefaultExpiration = TimeSpan.FromMilliseconds(
            GetEnvInt("LOGPORT_CACHE_DEFAULT_EXPIRATION_MS", (int)target.Cache.DefaultExpiration.TotalMilliseconds));

        if (target.Mode is LogMode.Agent && !target.Postgres.Use)
            throw new InvalidOperationException("At least one storage backend must be enabled.");

        return target;
    }


    private static string GetEnvString(string key, string defaultValue = "")
        => Environment.GetEnvironmentVariable(key) ?? defaultValue;

    private static bool GetEnvBool(string key, bool defaultValue = false)
        => bool.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;

    private static int GetEnvInt(string key, int defaultValue)
        => int.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;

    private static uint GetEnvUInt(string key, uint defaultValue)
        => uint.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
}