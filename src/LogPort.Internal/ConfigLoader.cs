using System.Text.Json;

namespace LogPort.Internal;

public static class ConfigLoader
{
    public static LogPortConfig Load()
    {
        var path = GetEnvString(EnvVars.ConfigPath, "config.json");
        bool fileExists = File.Exists(path);
        LogPortConfig? result = null;

        if (fileExists)
        {
            var config = File.ReadAllText("config.json");
            result = JsonSerializer.Deserialize<LogPortConfig>(config);
        }

        result ??= new LogPortConfig();

        if (!fileExists)
        {
            var opt = new JsonSerializerOptions() { WriteIndented = true, };
            File.WriteAllText(path, JsonSerializer.Serialize(result, opt));
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
        target.Port = GetEnvUInt(EnvVars.Port, target.Port);
        target.UpstreamUrl = GetEnvString(EnvVars.UpstreamUrl, target.UpstreamUrl)?.Trim('/');
        target.BatchSize = GetEnvInt(EnvVars.BatchSize, target.BatchSize);
        target.FlushIntervalMs = GetEnvInt(EnvVars.FlushIntervalMs, target.FlushIntervalMs);

        var modeStr = GetEnvString(EnvVars.Mode, target.Mode.ToString());
        if (Enum.TryParse<LogMode>(modeStr, true, out var mode))
            target.Mode = mode;

        target.ClientMaxReconnectDelay = TimeSpan.FromMilliseconds(
            GetEnvInt(EnvVars.ClientMaxReconnectDelayMs, (int)target.ClientMaxReconnectDelay.TotalMilliseconds));
        target.ClientHeartbeatInterval = TimeSpan.FromMilliseconds(
            GetEnvInt(EnvVars.ClientHeartbeatIntervalMs, (int)target.ClientHeartbeatInterval.TotalMilliseconds));
        target.ClientHeartbeatTimeout = TimeSpan.FromMilliseconds(
            GetEnvInt(EnvVars.ClientHeartbeatTimeoutMs, (int)target.ClientHeartbeatTimeout.TotalMilliseconds));

        target.Postgres.Use = GetEnvBool(EnvVars.UsePostgres, target.Postgres.Use);
        target.Postgres.Host = GetEnvString(EnvVars.PostgresHost, target.Postgres.Host);
        target.Postgres.Port = GetEnvInt(EnvVars.PostgresPort, target.Postgres.Port);
        target.Postgres.Database = GetEnvString(EnvVars.PostgresDatabase, target.Postgres.Database);
        target.Postgres.Username = GetEnvString(EnvVars.PostgresUsername, target.Postgres.Username);
        target.Postgres.Password = GetEnvString(EnvVars.PostgresPassword, target.Postgres.Password);
        target.Postgres.PartitionLength = GetEnvInt(EnvVars.PostgresPartitionLength, target.Postgres.PartitionLength);

        target.Docker.Use = GetEnvBool(EnvVars.UseDocker, target.Docker.Use);
        target.Docker.SocketPath = GetEnvString(EnvVars.DockerSocketPath, target.Docker.SocketPath);
        target.Docker.ExtractorConfigPath =
            GetEnvString(EnvVars.DockerExtractorConfigPath, target.Docker.ExtractorConfigPath);
        target.Docker.WatchAllContainers = GetEnvBool(EnvVars.DockerWatchAll, target.Docker.WatchAllContainers);

        target.Cache.UseRedis = GetEnvBool(EnvVars.UseRedis, target.Cache.UseRedis);
        target.Cache.RedisConnectionString =
            GetEnvString(EnvVars.RedisConnectionString, target.Cache.RedisConnectionString);
        target.Cache.DefaultExpiration = TimeSpan.FromMilliseconds(
            GetEnvInt(EnvVars.CacheDefaultExpirationMs, (int)target.Cache.DefaultExpiration.TotalMilliseconds));

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