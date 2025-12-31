using System.Text.Json;
using System.Text.Json.Serialization;

using LogPort.Internal.Configuration;

namespace LogPort.Internal;

public static class ConfigLoader
{
    /// <summary>
    /// Creates a <see cref="LogPortConfig"/> instance using a configuration file and environment variables.
    /// </summary>
    /// <returns>A <see cref="LogPortConfig"/> obtained from the configuration file and environment variables.</returns>
    public static LogPortConfig Load()
    {
        var path = GetEnvString(EnvVars.ConfigPath, "/conf/config.json");
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        bool fileExists = File.Exists(path);
        LogPortConfig? result = null;

        if (fileExists)
        {
            var config = File.ReadAllText(path);
            result = JsonSerializer.Deserialize<LogPortConfig>(config);
        }

        result ??= new LogPortConfig();

        var opt = new JsonSerializerOptions() { WriteIndented = true, };
        File.WriteAllText(path, JsonSerializer.Serialize(result, opt));

        LoadFromEnvironment(result);

        return result;
    }

    public static LogPortConfig LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new BaseLogEntryExtractorConfigJsonConverter()
            }
        };

        var result = JsonSerializer.Deserialize<LogPortConfig>(json, options)
                     ?? new LogPortConfig();

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
        target.AdminLogin = GetEnvString(EnvVars.AdminLogin, target.AdminLogin);
        target.AdminPassword = GetEnvString(EnvVars.AdminPassword, target.AdminPassword);

        var modeStr = GetEnvString(EnvVars.Mode, target.Mode.ToString());
        if (Enum.TryParse<LogMode>(modeStr, true, out var mode))
            target.Mode = mode;

        target.Postgres.Use = GetEnvBool(EnvVars.UsePostgres, target.Postgres.Use);
        target.Postgres.Host = GetEnvString(EnvVars.PostgresHost, target.Postgres.Host);
        target.Postgres.Port = GetEnvInt(EnvVars.PostgresPort, target.Postgres.Port);
        target.Postgres.Database = GetEnvString(EnvVars.PostgresDatabase, target.Postgres.Database);
        target.Postgres.Username = GetEnvString(EnvVars.PostgresUsername, target.Postgres.Username);
        target.Postgres.Password = GetEnvString(EnvVars.PostgresPassword, target.Postgres.Password);
        target.Postgres.PartitionLength = GetEnvInt(EnvVars.PostgresPartitionLength, target.Postgres.PartitionLength);

        target.Docker.Use = GetEnvBool(EnvVars.UseDocker, target.Docker.Use);
        target.Docker.SocketPath = GetEnvString(EnvVars.DockerSocketPath, target.Docker.SocketPath);
        target.Docker.WatchAllContainers = GetEnvBool(EnvVars.DockerWatchAll, target.Docker.WatchAllContainers);

        target.Cache.UseRedis = GetEnvBool(EnvVars.UseRedis, target.Cache.UseRedis);
        target.Cache.RedisConnectionString =
            GetEnvString(EnvVars.RedisConnectionString, target.Cache.RedisConnectionString);
        target.Cache.DefaultExpiration = TimeSpan.FromMilliseconds(
            GetEnvInt(EnvVars.CacheDefaultExpirationMs, (int)target.Cache.DefaultExpiration.TotalMilliseconds));

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